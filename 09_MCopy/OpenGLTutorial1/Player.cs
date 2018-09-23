using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1 {
	class Player {

		public Vector3 position;
		public int RenderDistance = 3;

		public Player(int x, int y, int z) {
			position = new Vector3(x, y, z);
		}
	}
}
