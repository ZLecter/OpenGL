using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1{
    class Program{

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

        public static string FragmentShader = @"
            #version 130
			uniform sampler2D texture;
            
			in vec2 uv;
			out vec4 fragment;
            
            void main(void){
				fragment = texture2D(texture, uv);
            }
        ";

        private static int width = 1280, height = 720;
        private static ShaderProgram program;
		private static VBO<Vector3> cube;
		private static VBO<int> cubeElements;

		private static VBO<Vector2> cubeUV;
		private static Texture crateTexture, rectTexture, pyTexture;

		private static int numShapes = 24;
		static int maxX = 6;
		static int maxY = 4;
		private static int[] texRandom;

        static void Main(string[] args){
			//Open GL init
			#region
			Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width,height);
            Glut.glutCreateWindow("Tu mama es maraca");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);

			Gl.ClearColor(0,0,0,1);

			//Alpha enabled
			Gl.Enable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.Blend);
			Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			#endregion

			//Load shader
			program = new ShaderProgram(VertexShader,FragmentShader);

            //Create perspective
            program.Use();
            program["projection_matrix"].SetValue(
                Matrix4.CreatePerspectiveFieldOfView(0.45f, 
                (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(
                Matrix4.LookAt(new Vector3(0, 0, 10), 
                Vector3.Zero, new Vector3(0, 1, 0)));

			//random texture for each 
			if(numShapes > maxX * maxY)
				numShapes = maxX * maxY;
			texRandom = new int[numShapes];
			crateTexture = new Texture("crate.jpg");
			rectTexture = new Texture("4922.jpg");
			pyTexture = new Texture("tile.jpg");
			Random r = new Random();
			for(int i = 0; i < numShapes; i++)
				texRandom[i] = r.Next(3);
			

			//Cube vertices and uv
			#region
			cube = new VBO<Vector3>(
				new Vector3[] {
					new Vector3(1,1,-1), new Vector3(-1,1,-1), new Vector3(-1,1,1), new Vector3(1,1,1),
					new Vector3(1,-1,1), new Vector3(-1,-1,1), new Vector3(-1,-1,-1), new Vector3(1,-1,-1),
					new Vector3(1,1,1), new Vector3(-1,1,1), new Vector3(-1,-1,1), new Vector3(1,-1,1),
					new Vector3(1,-1,-1), new Vector3(-1,-1,-1), new Vector3(-1,1,-1), new Vector3(1,1,-1),
					new Vector3(-1,1,1), new Vector3(-1,1,-1), new Vector3(-1,-1,-1), new Vector3(-1,-1,1),
					new Vector3(1,1,-1), new Vector3(1,1,1), new Vector3(1,-1,1), new Vector3(1,-1,-1),
				}
			);
			cubeElements = new VBO<int>(
				new int[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23 },
				BufferTarget.ElementArrayBuffer
			);
			cubeUV = new VBO<Vector2>(
				new Vector2[] {
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
				}
			);
			#endregion

			Glut.glutMainLoop();

        }

        private static void OnClose(){
            //Delete all resources on closing
			cube.Dispose();
            cubeElements.Dispose();
			cubeUV.Dispose();

			program.DisposeChildren = true;
            program.Dispose();
        }

        private static void OnRenderFrame(){

            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit 
                | ClearBufferMask.DepthBufferBit);

            //Use shader
            Gl.UseProgram(program);

			//Drawing cube
			#region
			int num = 0;
			for(int j = 0; j < maxY; j++) {
				for(int i = 0; i < maxX; i++) {
					if(num == numShapes)
						break;

					//select texture
					if(texRandom[num] == 0)
						Gl.BindTexture(crateTexture);
					else if(texRandom[num] == 1)
						Gl.BindTexture(rectTexture);
					else
						Gl.BindTexture(pyTexture);

					//set translation offset
					program["model_matrix"].SetValue(
						Matrix4.CreateScaling(new Vector3(0.5f, 0.5f, 0.5f)) *
						Matrix4.CreateTranslation(new Vector3(-3 + i * 1.1f, 1.5f - j * 1.1f, 0)));
					Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
					Gl.BindBufferToShaderAttribute(cubeUV, program, "vertexUV");
					Gl.BindBuffer(cubeElements);
					//Dibuja el cube
					Gl.DrawElements(BeginMode.Quads, cubeElements.Count,
						DrawElementsType.UnsignedInt, IntPtr.Zero);
					num++;
				}
				if(num == numShapes)
					break;
			}
				
			#endregion

			Glut.glutSwapBuffers();
        }

        private static void OnDisplay(){

        }

    }
}
