import unittest
import sys
import os
from unittest.mock import MagicMock, patch

# Add the 'Python' directory to sys.path so we can import modules
sys.path.append(os.path.join(os.path.dirname(__file__), '..', 'Python'))

class TestPythonCore(unittest.TestCase):
    def setUp(self):
        # Mock dependencies that might require hardware or specific OS
        self.mock_serial = MagicMock()
        self.mock_cv2 = MagicMock()
        
        # Apply patches
        self.serial_patcher = patch.dict('sys.modules', {'serial': self.mock_serial})
        self.cv2_patcher = patch.dict('sys.modules', {'cv2': self.mock_cv2})
        self.serial_patcher.start()
        self.cv2_patcher.start()

    def tearDown(self):
        self.serial_patcher.stop()
        self.cv2_patcher.stop()

    def test_import_main(self):
        """Test that main.py can be imported without syntax errors."""
        try:
            import main
        except ImportError as e:
            self.fail(f"Failed to import main.py: {e}")
        except Exception as e:
            self.fail(f"Importing main.py raised an exception: {e}")

    def test_mavlink_bridge_exists(self):
        """Test that mavlink_bridge function exists."""
        import main
        self.assertTrue(hasattr(main, 'mavlink_bridge'), "mavlink_bridge function missing")

    def test_camera_stream_exists(self):
        """Test that camera_stream function exists."""
        import main
        self.assertTrue(hasattr(main, 'camera_stream'), "camera_stream function missing")

if __name__ == '__main__':
    unittest.main()
