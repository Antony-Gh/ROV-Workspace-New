import unittest
import os
import sys
import subprocess
import platform
import logging

# Configuration
LOG_FILE = "test_results.log"
PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))
GUI_PROJECT_PATH = os.path.join(PROJECT_ROOT, "ROV GUI Control", "ROV GUI Control.csproj")
GUI_EXE_DEBUG_PATH = os.path.join(PROJECT_ROOT, "ROV GUI Control", "bin", "Debug", "ROV GUI Control.exe")
GUI_EXE_RELEASE_PATH = os.path.join(PROJECT_ROOT, "ROV GUI Control", "bin", "Release", "ROV GUI Control.exe")

def setup_logging():
    """Sets up logging to console and file."""
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(message)s",
        handlers=[
            logging.FileHandler(LOG_FILE, mode='w'),
            logging.StreamHandler(sys.stdout)
        ]
    )

def check_and_build_gui():
    """Checks if GUI exe exists, builds if missing (Windows only)."""
    if platform.system() != "Windows":
        logging.info("Not on Windows, skipping GUI build check.")
        return True

    if os.path.exists(GUI_EXE_DEBUG_PATH) or os.path.exists(GUI_EXE_RELEASE_PATH):
        logging.info("GUI executable found.")
        return True

    logging.info(f"GUI executable not found. Attempting to build {GUI_PROJECT_PATH}...")
    try:
        # Check if dotnet is available
        subprocess.run(["dotnet", "--version"], check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        
        # Build
        logging.info("Running: dotnet build")
        result = subprocess.run(
            ["dotnet", "build", GUI_PROJECT_PATH],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        
        if result.returncode == 0:
            logging.info("Build successful.")
            return True
        else:
            logging.error("Build failed.")
            logging.error(result.stdout)
            logging.error(result.stderr)
            return False

    except FileNotFoundError:
        logging.error("dotnet command not found. Please install .NET SDK.")
        return False
    except subprocess.CalledProcessError:
        logging.error("dotnet check failed.")
        return False
    except Exception as e:
        logging.error(f"An error occurred during build: {e}")
        return False

def run_tests():
    """Discovers and runs tests."""
    setup_logging()
    logging.info("Starting test run...")

    # Build step
    if not check_and_build_gui():
        logging.error("Build step failed or skipped with error. Aborting GUI tests might follow.")
        # We don't exit here because we might want to run Python unit tests anyway, 
        # but for a strict CI pipeline, we might want to fail. 
        # However, the user asked for a comprehensive script.
        # If build fails, GUI tests will fail in setUp.

    loader = unittest.TestLoader()
    start_dir = os.path.join(PROJECT_ROOT, 'tests')
    
    logging.info(f"Discovering tests in {start_dir}...")
    suite = loader.discover(start_dir)

    runner = unittest.TextTestRunner(verbosity=2)
    result = runner.run(suite)

    # Log summary
    logging.info("Test Run Complete")
    logging.info(f"Ran {result.testsRun} tests")
    if result.wasSuccessful():
        logging.info("Result: PASS")
        return 0
    else:
        logging.info(f"Result: FAIL (Errors: {len(result.errors)}, Failures: {len(result.failures)})")
        return 1

if __name__ == "__main__":
    exit_code = run_tests()
    sys.exit(exit_code)
