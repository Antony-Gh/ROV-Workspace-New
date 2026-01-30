import unittest
import os
import subprocess
import shutil
import platform

class TestSTM32Firmware(unittest.TestCase):
    def setUp(self):
        # Paths
        self.project_root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
        self.test_dir = os.path.join(self.project_root, 'STM32F405RGT6', 'test')
        self.build_dir = os.path.join(self.test_dir, 'build')

        # Check dependencies
        if shutil.which('cmake') is None:
            self.skipTest("cmake not found in PATH")
        
        # Determine likely compiler presence (gcc or cl)
        # We won't strictly fail here if not found, letting cmake fail instead,
        # but it's good to skip if obviously missing tooling? 
        # Actually letting cmake fail provides better error logs usually.

    def test_build_and_run_firmware_tests(self):
        """Configures, builds, and runs the STM32 firmware tests via CMake."""
        
        # 1. Create Build Directory
        os.makedirs(self.build_dir, exist_ok=True)

        # 2. Configure (cmake ..)
        print(f"\nConfiguring STM32 tests in {self.build_dir}...")
        try:
            subprocess.run(
                ['cmake', '..'], 
                cwd=self.build_dir, 
                check=True, 
                stdout=subprocess.PIPE, 
                stderr=subprocess.PIPE,
                text=True
            )
        except subprocess.CalledProcessError as e:
            self.fail(f"CMake Configuration Failed:\n{e.stderr}\n{e.stdout}")

        # 3. Build (cmake --build .)
        print("Building STM32 tests...")
        try:
            subprocess.run(
                ['cmake', '--build', '.'], 
                cwd=self.build_dir, 
                check=True, 
                stdout=subprocess.PIPE, 
                stderr=subprocess.PIPE,
                text=True
            )
        except subprocess.CalledProcessError as e:
            self.fail(f"CMake Build Failed:\n{e.stderr}\n{e.stdout}")

        # 4. Run Tests
        # Try finding the executable. 
        # Windows MSVC puts it in Debug/ or Release/
        # MinGW/Linux puts it in root of build dir.
        
        exe_name = "test_rov.exe" if platform.system() == "Windows" else "test_rov"
        
        possible_paths = [
            os.path.join(self.build_dir, exe_name),
            os.path.join(self.build_dir, "Debug", exe_name),
            os.path.join(self.build_dir, "Release", exe_name)
        ]
        
        test_exe = None
        for p in possible_paths:
            if os.path.exists(p):
                test_exe = p
                break
        
        if not test_exe:
             # Fallback to CTest if binary not found directly (though we prefer seeing output)
             print("Test binary not found standard paths, searching via CTest...")
             try:
                subprocess.run(['ctest'], cwd=self.build_dir, check=True)
                return # CTest passes
             except subprocess.CalledProcessError:
                self.fail("Test binary not found and CTest failed.")

        print(f"Running firmware tests: {test_exe}")
        try:
            result = subprocess.run(
                [test_exe], 
                check=True,
                stdout=subprocess.PIPE, 
                stderr=subprocess.PIPE,
                text=True
            )
            print("STM32 Test Output:\n" + result.stdout)
        except subprocess.CalledProcessError as e:
             self.fail(f"STM32 Firmware Tests Failed:\n{e.stdout}\n{e.stderr}")

if __name__ == '__main__':
    unittest.main()
