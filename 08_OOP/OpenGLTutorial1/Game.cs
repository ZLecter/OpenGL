using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1 {
	class Game {

		// Vertex Shader
		#region
		public static string VertexShader = @"
            #version 130
            
            in vec3 vertexPosition;
			in vec2 vertexUV;

			out vec2 uv;

            uniform mat4 projection_matrix;
            uniform mat4 view_matrix;
            uniform mat4 model_matrix;

            void main(void){
				uv = vertexUV;
                gl_Position = projection_matrix 
                * view_matrix * model_matrix *
                vec4(vertexPosition,1);
            }
        ";
		#endregion  vertex shader

		// Fragment Shader
		#region
		public static string FragmentShader = @"
            #version 130
			uniform sampler2D texture;
            
			in vec2 uv;
			out vec4 fragment;
            
            void main(void){
				fragment = texture2D(texture, uv);
            }
        ";
		#endregion

		private int width = 1280, height = 720;
		private ShaderProgram program;

		// private Texture crateTexture;
		private int numCircles = 5;
		private Circle[] circles;

		public Game() {
			Init();
		}

		public void Init() {
			//Open GL init
			#region
			Glut.glutInit();
			Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
			Glut.glutInitWindowSize(width, height);
			Glut.glutCreateWindow("Tu mama es maraca");

			Glut.glutIdleFunc(OnRenderFrame);
			Glut.glutDisplayFunc(OnDisplay);
			Glut.glutCloseFunc(OnClose);

			Gl.ClearColor(0, 0, 0, 1);

			//Alpha enabled
			Gl.Enable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.Blend);
			Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			#endregion

			//Load shader
			program = new ShaderProgram(VertexShader, FragmentShader);

			//Create perspective
			program.Use();
			program["projection_matrix"].SetValue(
				Matrix4.CreatePerspectiveFieldOfView(0.45f,
				(float)width / height, 0.1f, 1000f));
			program["view_matrix"].SetValue(
				Matrix4.LookAt(new Vector3(0, 0, 10),
				Vector3.Zero, new Vector3(0, 1, 0)));

			CreateModels();

			Glut.glutMainLoop();
		}

		private void CreateModels() {
			circles = new Circle[numCircles];
			for(int i = 0; i < numCircles; i++) {
				Vector3 pos = new Vector3(i-numCircles/2, 0, 0);
				Vector3 rot = new Vector3(0, 0, 1);
				circles[i] = new Circle(0.5f, 10, pos, rot, Vector3.One, i * 360/numCircles);
			}
		}

		private void OnClose() {
			foreach(Circle c in circles) {
				c.Dispose();
			}

			program.DisposeChildren = true;
			program.Dispose();
		}

		private void OnRenderFrame() {

			Gl.Viewport(0, 0, width, height);
			Gl.Clear(ClearBufferMask.ColorBufferBit
				| ClearBufferMask.DepthBufferBit);

			Gl.UseProgram(program);

			// Draw shit
			foreach(Circle c in circles) {
				c.Draw(program);
			}

			Glut.glutSwapBuffers();
		}

		private void OnDisplay() {

		}

	}
}
