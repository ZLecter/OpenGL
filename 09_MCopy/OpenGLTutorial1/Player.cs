using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1 {
	class Player {

		class Keys{
			public static char FRONT = 'w';
			public static char BACK = 's';
			public static char RIGHT = 'd';
			public static char LEFT = 'a';
			public static char UP = 'e';
			public static char DOWN = 'q';

			public static char RPY = 'x';
			public static char RNY = 'z';

		}

		public Vector3 position;
		public int RenderDistance = 5;
		public float mov = 1.0f;

		public Player(int x, int y, int z) {
			position = new Vector3(x, y, z);
		}

		public void KeyInput(byte key) {

			if(key == Keys.FRONT)
				Game.zCam -= mov;
			else if(key == Keys.BACK)
				Game.zCam += mov;

			if(key == Keys.RIGHT)
				Game.xCam += mov;
			else if(key == Keys.LEFT)
				Game.xCam -= mov;

			if(key == Keys.UP)
				Game.yCam -= mov;
			else if(key == Keys.DOWN)
				Game.yCam += mov;

			if(key == 'x')
				Game.ryCam += mov * 0.1f;
			else if(key == 'z')
				Game.ryCam -= mov * 0.1f;

			if(key == 'o') {
				RenderDistance++;

			} else if(key == 'p') {
				RenderDistance--;
				if(RenderDistance <= 0)
					RenderDistance = 1;
			}
		}
	}
}
