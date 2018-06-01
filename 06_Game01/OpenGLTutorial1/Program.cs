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
		private static VBO<Vector3> start, player;
		private static VBO<int> startElements, playerElements;
		private static VBO<Vector2> startUV, playerUV;
		private static Texture starTexture, playerTexture, enemyTexture;
		private static Vector3 playerColor;

		private static System.Diagnostics.Stopwatch watch;
		private static Random rng = new Random();
		
		private static bool autoRotate, lighting = true, fullscreen;
		private static bool left, right, up, down, shoot;
		private static float playerX = -2f, playerY = 0;
		private static float speed = 0.01f;
		private static float limX = 3.8f, limY = 1.8f;

		private static List<Shot> shots = new List<Shot>();
		private static float timeShot = -1;
		private static float speedShot = 0.3f;

		private static List<Shot> enemies = new List<Shot>();
		private static float timeEnemy = -1;
		private static float speedEnemy = 1f;


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
			playerTexture = new Texture("crate.jpg");
			enemyTexture = new Texture("tile.jpg");
			playerColor = new Vector3(1,1,1);
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

			//Player
			
			player = new VBO<Vector3>(
				new Vector3[]{
					new Vector3(0,0.7,0), new Vector3(1,0,0), new Vector3(0,-0.7,0), new Vector3(-0.5, 0, 0)
				}
			);
			playerElements = new VBO<int>(
				new int[] {0,1,2,3},
				BufferTarget.ElementArrayBuffer
			);
			playerUV = new VBO<Vector2>(
				new Vector2[] {
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
				}
			);
			


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
			else if(key == 32)
				shoot = true;
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
				else {
					Glut.glutPositionWindow(0, 0);
					Glut.glutReshapeWindow(1280, 720);
				}
			} else if(key == 32)
				shoot = false;
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

			//Perfom movement
			if(up)
				playerY += speed;
			if(down)
				playerY -= speed;
			if(left)
				playerX -= speed;
			if(right)
				playerX += speed;

			if(playerX > limX - 0.25f)
				playerX = limX - 0.25f;
			else if(playerX < -limX)
				playerX = -limX;
			//Clamp movement
			if(playerY > limY)
				playerY = limY;
			else if(playerY < -limY)
				playerY = -limY;

			if(shoot && timeShot <= 0) {
				Vector3 c = new Vector3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
				shots.Add(new Shot(playerX + 0.6f, playerY, c));
				timeShot = speedShot;
			}

			if(timeEnemy <= 0) {
				float ey = (rng.Next() < 0.5)? -1: 1;
				ey *= (float)rng.NextDouble() * limY;
				float ex = limX + 0.5f;
				enemies.Add(new Shot(ex, ey, Vector3.One));
				timeEnemy = speedEnemy;
			}

			Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit 
                | ClearBufferMask.DepthBufferBit);

            //Use shader
            Gl.UseProgram(program);
			program["color"].SetValue(playerColor);

			//Player Draw
			program["model_matrix"].SetValue(
				Matrix4.CreateScaling(new Vector3(0.5, 0.5, 0.5)) *
				Matrix4.CreateTranslation(new Vector3(playerX, playerY, 0))
			);
			Gl.BindTexture(playerTexture);
			Gl.BindBufferToShaderAttribute(player, program, "vertexPosition");
			Gl.BindBufferToShaderAttribute(playerUV, program, "vertexUV");
			Gl.BindBuffer(playerElements);

			Gl.DrawElements(BeginMode.Quads, playerElements.Count,
				DrawElementsType.UnsignedInt, IntPtr.Zero);
			


			
			//Calculate something
			Gl.BindTexture(starTexture);
			for(int i = shots.Count-1; i > 0; i--) {
				program["color"].SetValue(shots[i].color);

				program["model_matrix"].SetValue(
					Matrix4.CreateScaling(new Vector3(0.3f, 0.3f, 0.3f)) *
					Matrix4.CreateTranslation(new Vector3(shots[i].x, shots[i].y, 0))
				);
				Gl.BindBufferToShaderAttribute(start, program, "vertexPosition");
				Gl.BindBufferToShaderAttribute(startUV, program, "vertexUV");
				Gl.BindBuffer(startElements);

				Gl.DrawElements(BeginMode.Quads, startElements.Count,
					DrawElementsType.UnsignedInt, IntPtr.Zero);

				shots[i].x += Shot.speed;
				if(shots[i].x > limX)
					shots.RemoveAt(i);
			}

			Gl.BindTexture(enemyTexture);
			for(int i = enemies.Count - 1; i > 0; i--) {
				program["color"].SetValue(enemies[i].color);

				program["model_matrix"].SetValue(
					Matrix4.CreateScaling(new Vector3(0.5f, 0.5f, 0.5f)) *
					Matrix4.CreateTranslation(new Vector3(enemies[i].x, enemies[i].y, 0))
				);
				Gl.BindBufferToShaderAttribute(start, program, "vertexPosition");
				Gl.BindBufferToShaderAttribute(startUV, program, "vertexUV");
				Gl.BindBuffer(startElements);

				Gl.DrawElements(BeginMode.Quads, startElements.Count,
					DrawElementsType.UnsignedInt, IntPtr.Zero);

				enemies[i].x -= Shot.speed * 0.3f;
				if(enemies[i].x < -limX - 0.7f)
					enemies.RemoveAt(i);
			}

			timeShot -= deltaTime;
			timeEnemy -= deltaTime;

			Glut.glutSwapBuffers();
        }

        private static void OnDisplay(){

        }

    }
}

public class Shot{
	public float x, y;
	public Vector3 color;
	public static float speed = 0.01f;

	public Shot(float x, float y, Vector3 color){
		this.x = x;
		this.y = y;
		this.color = color;
	}

}