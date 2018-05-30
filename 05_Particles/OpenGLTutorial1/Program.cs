using System;
using Tao.FreeGlut;
using OpenGL;
using System.Collections.Generic;

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
			uniform vec3 color;			

			in vec2 uv;

			out vec4 fragment;
            
            void main(void){
				fragment = vec4(color * texture2D(texture, uv).xyz, 1);
            }
        ";

        private static int width = 1280, height = 720;

        private static ShaderProgram program;
		private static VBO<Vector3> start;
		private static VBO<int> startElements;
		private static VBO<Vector2> startUV;
		private static Texture starTexture;

		private static System.Diagnostics.Stopwatch watch;
		
		private static bool autoRotate, lighting = true, fullscreen;
		private static bool left, right, up, down;

		private static List<Star> s = new List<Star>();
		private static Random rng = new Random(Environment.TickCount);
		private static float theta = (float)Math.PI / 2, phi = (float)Math.PI / 2;
		private static int numStars = 600;

        static void Main(string[] args){
			//Open GL init
			#region
			Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(1280,720);
            Glut.glutCreateWindow("Tu mama es maraca");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);
			Glut.glutReshapeFunc(OnReshape);
			//Keyboard functions
			Glut.glutKeyboardFunc(OnKeyboardDown);
			Glut.glutKeyboardUpFunc(OnKeyboardUp);

			//Gl.ClearColor(0,0,0,1);

			//Alpha enabled
			Gl.Disable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.Blend);
			Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			
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
                new Vector3(0, 0, 0),
				new Vector3(0, 1, 0)));

			starTexture = new Texture("star.bmp");
			//start vertices and uv
			#region
			start = new VBO<Vector3>(
				new Vector3[] {
					new Vector3(-1, 1, 0), new Vector3(1, 1, 0),
					new Vector3(1, -1, 0), new Vector3(-1, -1, 0)
				}
			);
			startElements = new VBO<int>(
				new int[] { 0,1,2,3},
				BufferTarget.ElementArrayBuffer
			);
			startUV = new VBO<Vector2>(
				new Vector2[] {
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
				}
			);
			#endregion

			//Define stars
			float dist = 0;
			float r, g, b;
			for(int i = 0; i < numStars; i++) {
				dist = (float)i / numStars * 4f;
				r = (float)rng.NextDouble();
				g = (float)rng.NextDouble();
				b = (float)rng.NextDouble();
				s.Add(new Star(0, dist, new Vector3(r, g, b)));
			}

			watch = System.Diagnostics.Stopwatch.StartNew();

			Glut.glutMainLoop();

        }

		private static void OnReshape(int width, int height){
			Program.width = width;
			Program.height = height;
			program.Use();
			program["projection_matrix"].SetValue(
				Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width/height, 0.1f, 1000f));
		}

		private static void OnKeyboardDown(byte key, int x, int y){
			if(key == 'w')
				up = true;
			else if(key == 's')
				down = true;
			else if(key == 'd')
				right = true;
			else if(key == 'a')
				left = true;
			else if(key == 27)
				Glut.glutLeaveMainLoop();
		}

		private static void OnKeyboardUp(byte key, int x, int y){
			if(key == 'w')
				up = false;
			else if(key == 's')
				down = false;
			else if(key == 'd')
				right = false;
			else if(key == 'a')
				left = false;
			else if(key == 'l')
				lighting = !lighting;
			else if(key == 'f') {
				fullscreen = !fullscreen;
				if(fullscreen)
					Glut.glutFullScreen();
				else{
					Glut.glutPositionWindow(0,0);
					Glut.glutReshapeWindow(1280, 720);
				}
			} else if(key == 32)
				autoRotate = !autoRotate;
			else if(key == 27)
				Glut.glutLeaveMainLoop();
		}

        private static void OnClose(){
            //Delete all resources on closing
			start.Dispose();
            startElements.Dispose();
			startUV.Dispose();

			program.DisposeChildren = true;
            program.Dispose();
        }

        private static void OnRenderFrame(){
			//Clock tick used for animation
			watch.Stop();
			float deltaTime = (float)watch.ElapsedTicks/System.Diagnostics.Stopwatch.Frequency;
			watch.Restart();

			//Perfom rotations
			if(up)
				theta += deltaTime;
			if(down)
				theta -= deltaTime;
			if(left)
				phi -= deltaTime;
			if(right)
				phi += deltaTime;

			if(theta < 0)
				theta += (float)Math.PI * 2;

            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit 
                | ClearBufferMask.DepthBufferBit);

            //Use shader
            Gl.UseProgram(program);

			//Calculate something
			Gl.BindTexture(starTexture);
			Vector3 pos = 20 * new Vector3(
				(float)(Math.Cos(phi) * Math.Sin(theta)),
				(float)(Math.Cos(theta)),
				(float)(Math.Sin(phi) * Math.Sin(theta)));
			Vector3 upVector = ((theta % (Math.PI * 2)) > Math.PI) ? 
				new Vector3(0,1,0) :
				new Vector3(0,-1,0);

			program["view_matrix"].SetValue(Matrix4.LookAt(pos, Vector3.Zero, upVector));

			//Drawing stars
			#region
			for(int i = 0; i < numStars; i++) {
				program["model_matrix"].SetValue(
					Matrix4.CreateScaling(new Vector3(0.1f, 0.1f, 0.1f)) *
					Matrix4.CreateTranslation(new Vector3(s[i].dist * Math.Cos(s[i].dist), 0, 0)) *
					Matrix4.CreateRotationZ(s[i].angle)
				);
				// z = Math.Cos(s[i].angle + s[i].dist)
				// x = s[i].dist * Math.Cos(s[i].dist + s[i].angle)
				program["color"].SetValue(s[i].color);
				
				Gl.BindBufferToShaderAttribute(start, program, "vertexPosition");
				Gl.BindBufferToShaderAttribute(startUV, program, "vertexUV");
				Gl.BindBuffer(startElements);
			
				Gl.DrawElements(BeginMode.Quads, startElements.Count,
					DrawElementsType.UnsignedInt, IntPtr.Zero);

				s[i].angle += (float)i / s.Count * deltaTime * 2;
				s[i].dist -= 1f * deltaTime;

				if(s[i].dist < 0f){
					s[i].dist += 4f;
					s[i].color = new Vector3(
						(float)rng.NextDouble(),
						(float)rng.NextDouble(),
						(float)rng.NextDouble()
					);
				}

			}
			#endregion

			Glut.glutSwapBuffers();
        }

        private static void OnDisplay(){

        }

    }
}


public class Star{
	public float angle;
	public float dist;
	public Vector3 color;

	public Star(float angle, float dist, Vector3 color){
		this.angle = angle;
		this.dist = dist;
		this.color = color;
	}
}