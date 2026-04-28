import sys
import os
import bpy
import sys

current_file = bpy.data.texts["main.py"].filepath

current_dir = os.path.dirname(current_file)
parent_dir = os.path.dirname(current_dir) 

if parent_dir not in sys.path:
    sys.path.append(parent_dir)

import site

site.addsitedir(site.getusersitepackages())

from app.services.scene_editor import update_scene
from app.services.grpc_client import start_grpc
from app.services.log_init import setup_logging

setup_logging()
start_grpc()
bpy.app.timers.register(update_scene)