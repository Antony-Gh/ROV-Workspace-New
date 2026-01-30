import unittest
import os
import sys
import time
import subprocess
import platform

# Only import pywinauto if on Windows
PYWINAUTO_AVAILABLE = False
if platform.system() == "Windows":
    try:
        from pywinauto.application import Application
        from pywinauto import Desktop
        PYWINAUTO_AVAILABLE = True
    except ImportError:
        PYWINAUTO_AVAILABLE = False

class TestGUIWindows(unittest.TestCase):
    def setUp(self):
        if platform.system() != "Windows":
            self.skipTest("Skipping GUI tests on non-Windows platform")
        
        if not PYWINAUTO_AVAILABLE:
            self.fail("pywinauto not installed. Please run: pip install pywinauto")
        
        # Relative path to the executable (adjust based on where run_tests.py is executed)
        # Assuming run_tests.py is in root
        self.exe_path = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', 'ROV GUI Control', 'bin', 'Debug', 'ROV GUI Control.exe'))
        
        if not os.path.exists(self.exe_path):
             # Try Release folder if Debug not found
            self.exe_path = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', 'ROV GUI Control', 'bin', 'Release', 'ROV GUI Control.exe'))

        if not os.path.exists(self.exe_path):
            self.fail(f"Executable not found at {self.exe_path}. Build the solution first.")

        self.app = None

    def tearDown(self):
        """Ensure the application is closed after the test."""
        if self.app:
            try:
                self.app.kill()
            except Exception:
                pass
        
        # Double check with taskkill to be safe
        try:
            subprocess.run(["taskkill", "/F", "/IM", "ROV GUI Control.exe"], 
                           stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        except Exception:
            pass

    def test_launch_and_run(self):
        """Test that the application launches and runs for a few seconds."""
        print(f"Launching {self.exe_path}...")
        try:
            # Start the application
            self.app = Application(backend="uia").start(self.exe_path)
            
            # Wait for it to initialize (adjust timeout as needed)
            time.sleep(5)
            
            # Connect to the main window
            # Adjust title regex or class_name if needed. MainWindow.xaml usually has title "MainWindow" or set in XAML.
            # We'll try to connect to any window from this process.
            
            main_window = self.app.window(title_re=".*ROV.*") # Assuming "ROV" acts as wild card match based on project name
            
            if not main_window.exists():
                # Fallback to process connection check
                self.assertTrue(self.app.is_process_running(), "Process is not running after launch")
                print("Main window title might vary, but process is running.")
            else:
                self.assertTrue(main_window.exists(), "Main window does not exist")
                main_window.wait('visible', timeout=10)
                print("Main window found and visible.")

        except Exception as e:
            self.fail(f"GUI launch failed: {e}")

if __name__ == '__main__':
    unittest.main()
