using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;
using System.Drawing.Imaging;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Threading;

namespace IVISS.Classes
{
	class Stitch_Generator
	{
		public class Bitmap_Mine
		{
			public Image<Gray, byte> data;

			public int width;
			public int height;
			public int max;

			public int xstart;
			public int xend;

			public int ystart;
			public int yend;

			public Bitmap_Mine(int width, int height)
			{
				data = new Image<Gray, byte>(width, height);

				this.width = width;
				this.height = height;
				max = 255; ;

				xstart = 0;
				xend = width;

				ystart = 0;
				yend = height;
			}

			public Bitmap_Mine(string filename)
			{
				data = new Image<Gray, byte>(filename);

				this.width = data.Width;
				this.height = data.Height;
				max = 255; ;

				xstart = 0;
				xend = width;

				ystart = 0;
				yend = height;
			}

			public Bitmap_Mine(byte[,,] data)
			{
				this.data = new Image<Gray, byte>(data);

				this.width = this.data.Width;
				this.height = this.data.Height;
				max = 255; ;

				xstart = 0;
				xend = width;

				ystart = 0;
				yend = height;
			}
		}

		public class ColorBitmap_Mine
		{
			public Image<Bgr, byte> data;

			public int width;
			public int height;
			public int max;

			public int xstart;
			public int xend;

			public int ystart;
			public int yend;

			public ColorBitmap_Mine(int width, int height)
			{
				data = new Image<Bgr, byte>(width, height);

				this.width = width;
				this.height = height;
				max = 255; ;

				xstart = 0;
				xend = width;

				ystart = 0;
				yend = height;
			}

			public ColorBitmap_Mine(string filename)
			{
				data = new Image<Bgr, byte>(filename);

				this.width = data.Width;
				this.height = data.Height;
				max = 255; ;

				xstart = 0;
				xend = width;

				ystart = 0;
				yend = height;
			}

			public ColorBitmap_Mine(byte[,,] data)
			{
				this.data = new Image<Bgr, byte>(data);

				this.width = this.data.Width;
				this.height = this.data.Height;
				max = 255; ;

				xstart = 0;
				xend = width;

				ystart = 0;
				yend = height;
			}
		}

		// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		Capture _capture1 = null;
		int frame_count;

		System.Timers.Timer My_Timer = new System.Timers.Timer();

		private BlockingCollection<Bitmap> image_queue;

		BlockingCollection<ColorBitmap_Mine> image_color_undistorted;
		BlockingCollection<Bitmap_Mine> image_grey_undistorted;


		int NUM_THREADS = 2;
		private BlockingCollection<Bitmap>[] tmp_queue_distorted;
		BlockingCollection<ColorBitmap_Mine>[] tmp_queue_undistorted_color;
		BlockingCollection<Bitmap_Mine>[] tmp_queue_undistorted_grey;

		Bitmap_Mine W1;
		Bitmap_Mine W2;
		Bitmap_Mine W3;
		Bitmap_Mine W4;

		int[] x_dat;
		int[] y_dat;

		int w, h;

		ColorBitmap_Mine stitched_image;
		bool stitch_done = false;

		const int NSTRIPS = 6;

		int[] wt2 = {   1, 6, 10, 6, 1,
						6, 32, 51, 32, 6,
						10, 51, 82, 51, 10,
						6, 32, 51, 32, 6,
						1, 6, 10, 6, 1
					};

		public Stitch_Generator()
		{
			//TODO 5: Coonvert to 2d arrays for better access
			W1 = new Bitmap_Mine("W1.pgm");
			W2 = new Bitmap_Mine("W2.pgm");
			W3 = new Bitmap_Mine("W3.pgm");
			W4 = new Bitmap_Mine("W4.pgm");

			w = W1.data.Width;
			h = W1.data.Height;

			//Bitmap_Mine image = new Bitmap_Mine("Xlow.dat");

			//TODO 5: Coonvert to 2d arrays for better access
			var length = W1.data.Height * W1.data.Width;
			x_dat = new int[length];
			y_dat = new int[length];

			using (var reader = new BinaryReader(File.Open("XLow.dat", FileMode.Open)))
			{
				for (int i = 0; i < length; i++)
					x_dat[i] = reader.ReadInt32();
			}

			using (var reader = new BinaryReader(File.Open("YLow.dat", FileMode.Open)))
			{
				for (int i = 0; i < length; i++)
					y_dat[i] = reader.ReadInt32();
			}

			image_queue = new BlockingCollection<Bitmap>();

			image_color_undistorted = new BlockingCollection<ColorBitmap_Mine>();
			image_grey_undistorted = new BlockingCollection<Bitmap_Mine>();

			tmp_queue_distorted = new BlockingCollection<Bitmap>[NUM_THREADS];
			tmp_queue_undistorted_color = new BlockingCollection<ColorBitmap_Mine>[NUM_THREADS];
			tmp_queue_undistorted_grey = new BlockingCollection<Bitmap_Mine>[NUM_THREADS];

			for (int i = 0; i < NUM_THREADS; i++)
			{
				tmp_queue_distorted[i] = new BlockingCollection<Bitmap>();
				tmp_queue_undistorted_color[i] = new BlockingCollection<ColorBitmap_Mine>();
				tmp_queue_undistorted_grey[i] = new BlockingCollection<Bitmap_Mine>();
			}
		}

		public Stitch_Generator(string videoPath)
		{
			//TODO 5: Coonvert to 2d arrays for better access
			W1 = new Bitmap_Mine("W1.pgm");
			W2 = new Bitmap_Mine("W2.pgm");
			W3 = new Bitmap_Mine("W3.pgm");
			W4 = new Bitmap_Mine("W4.pgm");

			w = W1.data.Width;
			h = W1.data.Height;

			//Bitmap_Mine image = new Bitmap_Mine("Xlow.dat");

			//TODO 5: Coonvert to 2d arrays for better access
			var length = W1.data.Height * W1.data.Width;
			x_dat = new int[length];
			y_dat = new int[length];

			using (var reader = new BinaryReader(File.Open("XLow.dat", FileMode.Open)))
			{
				for (int i = 0; i < length; i++)
					x_dat[i] = reader.ReadInt32();
			}

			using (var reader = new BinaryReader(File.Open("YLow.dat", FileMode.Open)))
			{
				for (int i = 0; i < length; i++)
					y_dat[i] = reader.ReadInt32();
			}

			try
			{
				_capture1 = new Capture(videoPath);

				//TODO 2: Make frame count right
				frame_count = (int)_capture1.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_COUNT) / 1;

				Console.WriteLine("Frame count 1:{0} \n", frame_count);
			}
			catch (NullReferenceException except)
			{
				//ColorBitmap_Mine image = new ColorBitmap_Mine("C:\\CombineImage\\outPutVer.ppm");
				//imageBox1.Image = image;
				return;
			}

			image_queue = new BlockingCollection<Bitmap>();

			image_color_undistorted = new BlockingCollection<ColorBitmap_Mine>();
			image_grey_undistorted = new BlockingCollection<Bitmap_Mine>();

			tmp_queue_distorted = new BlockingCollection<Bitmap>[NUM_THREADS];
			tmp_queue_undistorted_color = new BlockingCollection<ColorBitmap_Mine>[NUM_THREADS];
			tmp_queue_undistorted_grey = new BlockingCollection<Bitmap_Mine>[NUM_THREADS];

			for (int i = 0; i < NUM_THREADS; i++)
			{
				tmp_queue_distorted[i] = new BlockingCollection<Bitmap>();
				tmp_queue_undistorted_color[i] = new BlockingCollection<ColorBitmap_Mine>();
				tmp_queue_undistorted_grey[i] = new BlockingCollection<Bitmap_Mine>();
			}
		}

		void decode_all_frames()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			for (int i = 0; i < frame_count; i++)
			{
				if (i % 200 == 0)
					Console.WriteLine("Frames added {0}: ", i);

				//! Works like a charm!
				//! (i.e. decodes at full speed, and not illegal memory accesses, and disposes everything automaticaly
				using (var image3 = _capture1.QueryFrame())
				{
					////var tmp_img_2 = new Bitmap(image.Bitmap);
					var tmp_img3 = image3.ToBitmap();
					image_queue.Add(tmp_img3);
				}

				//System.Threading.Thread.Sleep(10);
			}

			// No images are going to be added
			image_queue.CompleteAdding();

			sw.Stop();
			Console.WriteLine("Video decode time = {0}", sw.ElapsedMilliseconds);

			// Reset video to start
			_capture1.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);
		}

		// Cleaned up code
		void undistort_one_frame(Bitmap image_in, ColorBitmap_Mine CBOut, Bitmap_Mine BOut)
		{
			//ColorBitmap_Mine CBOut = new ColorBitmap_Mine(W1.Width, W1.Height);       // Undistorted (color)
			//Bitmap_Mine BOut = new Bitmap_Mine(W1.Width, W1.Height);       // Undistorted (grey)

			Bitmap bmp = image_in;
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, image_in.Width, image_in.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			unsafe
			{
				int i, j, w1, w2, w3, w4, xl, xh, yl, yh;
				int width_orig_img = image_in.Width;

				byte* ptr = (byte*)data.Scan0;

				for (i = 0; i < h; i++)
				{
					for (j = 0; j < w; j++)
					{
						//TODO 8: Coonvert to 2d arrays for better access
						w1 = W1.data.Data[i, j, 0];
						w2 = W2.data.Data[i, j, 0];
						w3 = W3.data.Data[i, j, 0];
						w4 = W4.data.Data[i, j, 0];

						xh = y_dat[i * w + j];
						xl = xh - 1;

						yh = x_dat[i * w + j];
						yl = yh - 1;

						byte tmp;

						tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 0] +
															   w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 0] +
															   w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 0] +
															   w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 0]) >> 7));
						CBOut.data.Data[i, j, 0] = (byte)tmp;

						tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 1] +
															   w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 1] +
															   w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 1] +
															   w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 1]) >> 7));
						CBOut.data.Data[i, j, 1] = (byte)tmp;

						tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 2] +
															   w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 2] +
															   w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 2] +
															   w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 2]) >> 7));
						CBOut.data.Data[i, j, 2] = (byte)tmp;

						// Convert to grey-scale image
						// Make this line right (Make a grey-scale image) [Works fine now]
						BOut.data.Data[i, j, 0] = (byte)((38 * CBOut.data.Data[i, j, 0] + 75 * CBOut.data.Data[i, j, 1] + 15 * CBOut.data.Data[i, j, 2]) >> 7); // >>7 is division by 128
					}
				}
			}

			//image_in.Bitmap.UnlockBits(data);
			bmp.UnlockBits(data);
		}

		//? Doesn't work
		void undistort_one_frame_fast(Bitmap image_in, ColorBitmap_Mine CBOut, Bitmap_Mine BOut)
		{
			//ColorBitmap_Mine CBOut = new ColorBitmap_Mine(W1.Width, W1.Height);       // Undistorted (color)
			//Bitmap_Mine BOut = new Bitmap_Mine(W1.Width, W1.Height);       // Undistorted (grey)

			Bitmap bmp = image_in;
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, image_in.Width, image_in.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);




			Bitmap W1_bmp = W1.data.Bitmap;
			BitmapData W1_data = W1_bmp.LockBits(new Rectangle(0, 0, W1_bmp.Width, W1_bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);


			unsafe
			{
				int i, j, w1, w2, w3, w4, xl, xh, yl, yh;
				int width_orig_img = image_in.Width;

				byte* ptr = (byte*)data.Scan0;

				byte* W1_ptr = (byte*)W1_data.Scan0;

				for (i = 0; i < h; i++)
				{
					for (j = 0; j < w; j++)
					{
						//TODO 8: Coonvert to 2d arrays for better access
						//w1 = W1.data.Data[i, j, 0];
						//w1 = (int)W1_ptr[i*W1_bmp.Width + j];
						w1 = (int)W1_ptr[i * w + j];

						w2 = W2.data.Data[i, j, 0];
						w3 = W3.data.Data[i, j, 0];
						w4 = W4.data.Data[i, j, 0];

						xh = y_dat[i * w + j];
						xl = xh - 1;

						yh = x_dat[i * w + j];
						yl = yh - 1;

						byte tmp;

						tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 0] +
															   w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 0] +
															   w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 0] +
															   w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 0]) >> 7));
						CBOut.data.Data[i, j, 0] = (byte)tmp;

						tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 1] +
															   w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 1] +
															   w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 1] +
															   w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 1]) >> 7));
						CBOut.data.Data[i, j, 1] = (byte)tmp;

						tmp = (byte)Math.Max(0, Math.Min(255, (w1 * ptr[(xl * width_orig_img * 3) + (yl * 3) + 2] +
															   w3 * ptr[(xh * width_orig_img * 3) + (yl * 3) + 2] +
															   w2 * ptr[(xl * width_orig_img * 3) + (yh * 3) + 2] +
															   w4 * ptr[(xh * width_orig_img * 3) + (yh * 3) + 2]) >> 7));
						CBOut.data.Data[i, j, 2] = (byte)tmp;

						// Convert to grey-scale image
						// Make this line right (Make a grey-scale image) [Works fine now]
						BOut.data.Data[i, j, 0] = (byte)((38 * CBOut.data.Data[i, j, 0] + 75 * CBOut.data.Data[i, j, 1] + 15 * CBOut.data.Data[i, j, 2]) >> 7); // >>7 is division by 128
					}
				}
			}

			//image_in.Bitmap.UnlockBits(data);
			bmp.UnlockBits(data);

			bmp.UnlockBits(W1_data);
		}

		void undistort_all_frames()
		{
			// TODO 9: Use "Dataflow (Task Parallel Library)" to make it run in parallel

			Stopwatch sw = new Stopwatch();
			sw.Start();

			ColorBitmap_Mine CBOut = new ColorBitmap_Mine(W1.data.Width, W1.data.Height);   // Undistorted (color)
			Bitmap_Mine BOut = new Bitmap_Mine(W1.data.Width, W1.data.Height);              // Undistorted (grey)

			int i = 0;

			// Run loop until queue is empty
			//for (int i = 0; i < frame_count; i++)
			while (!image_queue.IsCompleted)
			{
				if (i % 200 == 0)
					Console.WriteLine("\t\t\t Frames taken {0}: ", i);

				string name1 = String.Format(@"images\{0}.jpg", i);


				// This section works
				// Take images from queue
				Bitmap distorted_img = image_queue.Take();

				// Create undistorted image
				undistort_one_frame(distorted_img, CBOut, BOut);
				//undistort_one_frame_fast(distorted_img, CBOut, BOut);

				// Save both 
				image_color_undistorted.Add(new ColorBitmap_Mine(CBOut.data.Data));
				image_grey_undistorted.Add(new Bitmap_Mine(BOut.data.Data));

				//! Save image to file
				//CBOut.data.Save(name1);
				//BOut.data.Save(name1);

				// Dispose original image
				distorted_img.Dispose();

				i++;
			}

			// Dispose and reset the queue
			image_queue.Dispose();

			try
			{
				image_color_undistorted.CompleteAdding();
				image_grey_undistorted.CompleteAdding();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			sw.Stop();
			Console.WriteLine("Undistort whole video time = {0}", sw.ElapsedMilliseconds);
		}

		void combine_queues()
		{
			int i = 0;
			bool isDone = true;

			while (isDone)
			{
				try
				{
					if (!tmp_queue_undistorted_color[i % NUM_THREADS].IsCompleted)
					{
						var t1 = tmp_queue_undistorted_color[i % NUM_THREADS].Take();
						var t2 = tmp_queue_undistorted_grey[i % NUM_THREADS].Take();

						image_color_undistorted.Add(t1);
						image_grey_undistorted.Add(t2);
					}

					i++;

					isDone = true;
					for (int j = 0; j < NUM_THREADS; j++)
					{
						isDone = isDone && !tmp_queue_undistorted_color[j % NUM_THREADS].IsCompleted;
					}
				}
				catch (InvalidOperationException e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine("Error on 'Take()'");
					break;
				}
			}

			//for (int j = 0; j < NUM_THREADS; j++)
			//{
			//    tmp_queue_undistorted_color[j].CompleteAdding();
			//    tmp_queue_undistorted_grey[j].CompleteAdding();
			//}

			
			try
			{
				image_color_undistorted.CompleteAdding();
				image_grey_undistorted.CompleteAdding();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private void process_tmp_distorted_queue(int n)
		{
			ColorBitmap_Mine CBOut = new ColorBitmap_Mine(W1.data.Width, W1.data.Height);   // Undistorted (color)
			Bitmap_Mine BOut = new Bitmap_Mine(W1.data.Width, W1.data.Height);              // Undistorted (grey)

			Console.WriteLine("\t\t\t Task {0} started with n: {1}", Task.CurrentId, n);

			int i = 0;

			while (!tmp_queue_distorted[n].IsCompleted)
			{
				//Console.WriteLine("\t Thread {0}: {1}", n, i);

				try
				{
					Bitmap distorted_img = tmp_queue_distorted[n].Take();

					// Create undistorted image
					undistort_one_frame(distorted_img, CBOut, BOut);



					tmp_queue_undistorted_color[n].Add(new ColorBitmap_Mine(CBOut.data.Data));
					tmp_queue_undistorted_grey[n].Add(new Bitmap_Mine(BOut.data.Data));

					i++;
				}
				catch (InvalidOperationException e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine("Error on 'Take()'");
					break;
				}
			}

            //tmp_queue_distorted[n].Dispose();
            try
            {
				tmp_queue_undistorted_color[n].CompleteAdding();
				tmp_queue_undistorted_grey[n].CompleteAdding();
			}
            catch (Exception e)
            {
				Console.WriteLine(e.Message);
			}
			

			Console.WriteLine("\t\t\t Task {0} ended with n: {1}", Task.CurrentId, n);
		}

		void undistort_all_frames_parallel()
		{
			
			Stopwatch sw = new Stopwatch();
			sw.Start();

			for (int j = 0; j < NUM_THREADS; j++)
			{
				int tmp = j;
				Task.Factory.StartNew(() => process_tmp_distorted_queue(tmp));
			}

			Task.Factory.StartNew(() => combine_queues());

			int i = 0;

			// Run loop until queue is empty
			while (!image_queue.IsCompleted)
			{
				try
				{
					Bitmap distorted_img = image_queue.Take();

					tmp_queue_distorted[i % NUM_THREADS].Add(distorted_img);
					//tmp_queue_distorted[0].Add(distorted_img);

					i++;
				}
				catch (InvalidOperationException e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine("Error on 'Take()'");
					break;
				}
			}

			try
			{
				for (int j = 0; j < NUM_THREADS; j++)
				{
					tmp_queue_distorted[j].CompleteAdding();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			sw.Stop();
			Console.WriteLine("Undistort whole video time = {0}", sw.ElapsedMilliseconds);
		}

		// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		Bitmap_Mine reduce(Bitmap_Mine b)
		{
			int[,] wt = {   {1, 6, 10, 6, 1},
							{6, 32, 51, 32, 6},
							{10, 51, 82, 51, 10},
							{6, 32, 51, 32, 6},
							{1, 6, 10, 6, 1}
						};

			int h = b.data.Height;
			int w = b.data.Width;

			int hOut = (int)((h - 4) / 2);
			int wOut = (int)((w - 4) / 2);

			var bTmp = new Bitmap_Mine(wOut, hOut);

			for (int i = 0; i < hOut; i++)
			{
				for (int j = 0; j < wOut; j++)
				{
					int data = 0, color = 0;
					for (int m = -2; m <= 2; m++)
					{
						for (int n = -2; n <= 2; n++)
						{
							data = b.data.Data[2 * i + m + 2, 2 * j + n + 2, 0];
							color += wt[m + 2, n + 2] * data;
						}
					}
					//bOut->data[i][j] = color/400;
					bTmp.data.Data[i, j, 0] = (byte)(color >> 9);      // divsion by 512
				}
			}

			return bTmp;
		}

		Bitmap_Mine reduce_fast(Bitmap_Mine b)
		{
			Bitmap bmp = b.data.Bitmap;
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);


			int h = b.data.Height;
			int w = b.data.Width;

			int hOut = (int)((h - 4) / 2);
			int wOut = (int)((w - 4) / 2);

			var bTmp = new Bitmap_Mine(wOut, hOut);

			unsafe
			{
				byte* ptr = (byte*)data.Scan0;

				fixed (int* ptr_wt2 = wt2)
					for (int i = 0; i < hOut; i++)
					{
						for (int j = 0; j < wOut; j++)
						{
							int data_tmp = 0, color = 0;
							for (int m = -2; m <= 2; m++)
							{
								for (int n = -2; n <= 2; n++)
								{
									//data_tmp = b.data.Data[2 * i + m + 2, 2 * j + n + 2, 0];

									int x = 2 * i + m + 2;
									int y = 2 * j + n + 2;

									//data_tmp = b.data.Data[x, y, 0];

									data_tmp = ptr[x * w + y];

									//color += wt[m + 2, n + 2] * data;
									//color += wt2[(5 * (m + 2)) + (n + 2)] * data_tmp;     // Works

									color += *(ptr_wt2 + (5 * (m + 2)) + (n + 2)) * data_tmp;   // Works faster
								}
							}
							//bOut->data[i][j] = color/400;
							bTmp.data.Data[i, j, 0] = (byte)(color >> 9);      // divsion by 512
						}
					}
			}

			bmp.UnlockBits(data);

			return bTmp;
		}

		void warp(ref Bitmap_Mine b1, ref Bitmap_Mine b2, double xTrans)
		{
			int i, j, tr;

			if (xTrans < -0.1 * b2.height && xTrans > 0.8 * b2.height)
				return;

			tr = (int)Math.Round(xTrans);

			if (tr == 0)
				return;
			//else if(tr>0)
			else if ((b1.xstart + tr) >= 0)
			{
				b1.xstart += tr;
				b1.height -= tr;
				b2.height -= tr;
				b2.xend -= tr;
			}
			else
			{
				for (i = b1.height - 1; i >= 0; i--)
				{
					for (j = 0; j < b1.width; j++)
					{
						if ((i + tr) >= 0)
							b1.data.Data[i, j, 0] = b1.data.Data[i + tr, j, 0];
						else
							b1.data.Data[i, j, 0] = 0;
					}
				}
			}
		}

		void computeDerivatives(Bitmap_Mine b1, Bitmap_Mine b2, ref int[,] fx, ref int[,] ft)
		{
			int i,
				j,
				x,
				value = 0,
				h,
				w,
				istart,
				xstart,
				iend,
				jend,
				jstart;

			istart = b1.xstart + 1;
			xstart = b2.xstart + 1;
			iend = b1.height - 2;
			jend = b1.width - 2;

			jstart = 2;
			h = iend - istart + 1;
			w = jend - jstart + 1;

			fx = new int[b1.data.Height, b1.data.Width];
			ft = new int[b1.data.Height, b1.data.Width];

			for (i = istart, x = xstart; i <= iend; i++, x++)
			{
				for (j = jstart; j <= jend; j++)
				{
					value = b1.data.Data[i, j - 1, 0] + b1.data.Data[i, j, 0]
							- b1.data.Data[i - 1, j - 1, 0] - b1.data.Data[i - 1, j, 0]
							+ b2.data.Data[x, j - 1, 0] + b2.data.Data[x, j, 0]
							- b2.data.Data[x - 1, j - 1, 0] - b2.data.Data[x - 1, j, 0];
					fx[i - istart, j] = value;


					value = b2.data.Data[x - 1, j - 1, 0] + b2.data.Data[x - 1, j, 0] + b2.data.Data[x, j - 1, 0]
						+ b2.data.Data[x, j, 0] - b1.data.Data[i - 1, j - 1, 0] - b1.data.Data[i - 1, j, 0]
						- b1.data.Data[i, j - 1, 0] - b1.data.Data[i, j, 0];
					ft[i - istart, j] = value;
				}
			}
		}

		double LSq_computeXTrans(Bitmap_Mine b1, Bitmap_Mine b2, int fNum, int transEst, int pLevel)
		{
			int i,
				j,
				maxIter = 4,
				iterations = 0;

			double xTrans = 0,
				   dxTrans = 0;
			double[] trans = { 0, 0, 0, 0, 0, 0 };

			/////////////////////////////////////////////////////////

			Bitmap_Mine[] b1Pyr = new Bitmap_Mine[pLevel];
			Bitmap_Mine[] b2Pyr = new Bitmap_Mine[pLevel];

			int[][,] fx = new int[pLevel][,];
			int[][,] ft = new int[pLevel][,];

			// //////////////////////////// -- Gaussian Pyramids Calculation -- ////////////////////////////

			b1Pyr[0] = b1;
			b2Pyr[0] = b2;

			string name1;

			Task t1 = Task.Factory.StartNew(() =>
			{
				for (int k = 1; k < pLevel; k++)
				{
					//b1Pyr[i] = reduce(b1Pyr[i - 1]);
					//b2Pyr[i] = reduce(b2Pyr[i - 1]);

					b1Pyr[k] = reduce_fast(b1Pyr[k - 1]);
					//b2Pyr[i] = reduce_fast(b2Pyr[i - 1]);
				}
			});

			for (i = 1; i < pLevel; i++)
			{
				//b1Pyr[i] = reduce(b1Pyr[i - 1]);
				//b2Pyr[i] = reduce(b2Pyr[i - 1]);

				//b1Pyr[i] = reduce_fast(b1Pyr[i - 1]);
				b2Pyr[i] = reduce_fast(b2Pyr[i - 1]);

				name1 = String.Format(@"images\{0}_level({1}).jpg", fNum, i);
				//b1Pyr[i].data.Save(name1);

				transEst /= 2;
			}

			t1.Wait();

			// //////////////////////////// --  -- ////////////////////////////

			if (transEst < 3)
				xTrans = 0;
			else
				xTrans = transEst;

			// //////////////////////////// -- Derivatives Calculation -- ////////////////////////////

			for (i = pLevel - 1; i > 0; i--)
			{
				if (i <= 1)
					iterations = 10;
				else
					iterations = 0;

				warp(ref b2Pyr[i], ref b1Pyr[i], xTrans);       // initial Translation warping on b1

				do
				{
					computeDerivatives(b1Pyr[i], b2Pyr[i], ref fx[i], ref ft[i]);
					/*if(i<=1)
						dxTrans = subComputeXTrans(fx[i], ft[i], trans, dxTrans, mean, sd);
					else if(i==2)
						dxTrans = subComputeXTrans(fx[i], ft[i], trans, 0, mean, sd);
					else*/


					//?dxTrans = subComputeXTrans_short(fx[i], ft[i], xTrans);          // Doesn't work
					dxTrans = subComputeXTrans_long(fx[i], ft[i], xTrans);      // This one works great


					//dxTrans = subComputeXTrans(fx[i], ft[i], xTrans, trans, 0, NULL, NULL, frameStart);

					xTrans += dxTrans;

					if (++iterations < maxIter)
						warp(ref b2Pyr[i], ref b1Pyr[i], dxTrans);


					if (dxTrans < 0.2 && dxTrans > -0.2 && iterations > 1)
					{
						break;
					}

				} while (iterations < maxIter);

				xTrans *= 2;
			}

			// //////////////////////////// --  -- ////////////////////////////

			return xTrans;
		}

		double subComputeXTrans_short(int[,] Fx, int[,] Ft, double xTrans)
		{
			int oldTr = (int)xTrans;

			int A = 0, B = 0,
				rows = Fx.GetLength(0),
				cols = Fx.GetLength(1);

			double trans = 0;

			{
				for (int i = 0; i < rows; i++)
					for (int j = 0; j < cols; j++)
					{
						A += Fx[i, j] * Fx[i, j];
						B -= Fx[i, j] * Ft[i, j];
					}
			}

			trans = A != 0 ? (double)B / A : 0;

			return trans;
		}

		double computeTrans(double[] trans, int n)   // n is 6 here
		{
			int i, j, votInd;
			double diff, minDiff = 10000, xTrans;

			for (i = 0; i < n; i++)
			{
				double temp = trans[i];
				int ind = i;
				for (j = i; j < n; j++)
				{
					if (trans[j] < temp)
					{
						temp = trans[j];
						ind = j;
					}
				}
				trans[ind] = trans[i];
				trans[i] = temp;
			}

			if ((trans[n - 1] - trans[0]) < 10)
			{
				votInd = n - 1;
				for (i = n - 1; i > 1; i--)
				{
					if ((trans[i] - trans[i - 1]) < 2)
					{
						votInd = i;
					}
					if ((trans[i] - trans[i - 1]) < 2 && (trans[i - 1] - trans[i - 2]) <= 1)
					{
						votInd = i;
						break;
					}
				}
				xTrans = trans[votInd];
			}
			else
			{
				xTrans = trans[n - 1];
				for (i = n - 1; i > 0; i--)
				{
					diff = trans[i - 1] - trans[i];
					if (diff < minDiff)
					//if(diff<minDiff && trans[i-1] )
					{
						minDiff = diff;
						xTrans = trans[i - 1];
					}
				}

			}
			//printf("%d ", votInd);
			return xTrans;
		}

		double subComputeXTrans_long(int[,] Fx, int[,] Ft, double xTrans)
		{
			int NSTRIPS = 6;

			int[] A = { 0, 0, 0, 0, 0, 0 };
			int[] B = { 0, 0, 0, 0, 0, 0 };

			int rows = Fx.GetLength(0),
				cols = Fx.GetLength(1),
				a, b,
				cols1, cols2, cols3, cols4, cols5;

			double[] trans2 = { 0, 0, 0, 0, 0, 0 };
			double votTrans;   //, diff, minDiff=10000;

			double[] trans = { 0, 0, 0, 0, 0, 0 };

			cols1 = cols / NSTRIPS;
			cols2 = 2 * cols1;
			cols3 = 3 * cols1;
			cols4 = 4 * cols1;
			cols5 = 5 * cols1;

			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					a = Fx[i, j] * Fx[i, j];
					b = Fx[i, j] * Ft[i, j];

					if (j < cols1)
					{
						A[0] += a;
						B[0] -= b;
					}
					else if (j < cols2)
					{
						A[1] += a;
						B[1] -= b;
					}
					else if (j < cols3)
					{
						A[2] += a;
						B[2] -= b;
					}
					else if (j < cols4)
					{
						A[3] += a;
						B[3] -= b;
					}
					else if (j < cols5)
					{
						A[4] += a;
						B[4] -= b;
					}
					else
					{
						A[5] += a;
						B[5] -= b;
					}
				}
			}       // end for i


			for (int i = 0; i < NSTRIPS; i++)
				trans2[i] = A[i] != 0 ? (double)B[i] / A[i] : 0;


			votTrans = computeTrans(trans2, NSTRIPS);

			for (int i = 0; i < NSTRIPS; i++)
				trans[i] = xTrans + trans2[i];

			if (votTrans > (rows - 3))
				votTrans = rows - 3;
			else if (votTrans < -10)
				votTrans = -10;

			return votTrans;
		}

		ColorBitmap_Mine constructVerMosaic(ColorBitmap_Mine B1, ColorBitmap_Mine B2, double xTrans, double totTrans, ref ColorBitmap_Mine bOut)
		{
			int b1 = (int)Math.Floor(xTrans),
				bTot = (int)Math.Ceiling(totTrans),
				b2 = 0,
				w1, w2,
				totW,
				B1Ht = 0;

			// If 'bOut' fills up, allocate more space
			if (bOut.data.Height <= B1.height + b1)
			{
				var bOutNew = new ColorBitmap_Mine(bOut.data.Width, bOut.data.Height * 2);

				bOutNew.data.ROI = new Rectangle(0, 0, bOut.data.Width, bOut.data.Height);
				bOut.data.CopyTo(bOutNew.data);             // Copy data from smaller image to the larger one
				bOutNew.data.ROI = Rectangle.Empty;

				bOutNew.width = bOut.width;
				bOutNew.height = bOut.height;

				bOut = bOutNew;
			}

			B1Ht = B1.height;
			bOut.height = B1.height + b1;

			// ///////////////

			b2 = bTot + b1;
			int i2 = 0;

			if (b2 < B1Ht)
			{
				totW = B1Ht - b2;

				for (int i = b2; i < B1Ht; i++)
				{
					w1 = B1Ht - i - 1;
					w2 = i - b2 + 1;
					i2 = i - b2;

					for (int j = 0; j < bOut.width; j++)
					{
						bOut.data.Data[i, j, 0] = (byte)((w1 * B1.data.Data[i, j, 0] + w2 * B2.data.Data[i2, j, 0]) / totW);
						bOut.data.Data[i, j, 1] = (byte)((w1 * B1.data.Data[i, j, 1] + w2 * B2.data.Data[i2, j, 1]) / totW);
						bOut.data.Data[i, j, 2] = (byte)((w1 * B1.data.Data[i, j, 2] + w2 * B2.data.Data[i2, j, 2]) / totW);
					}
				}

				for (int i = B1Ht; i < bOut.height; i++)
				{
					i2 = i - b2;
					for (int j = 0; j < bOut.width; j++)
					{
						bOut.data.Data[i, j, 0] = B2.data.Data[i2, j, 0];
						bOut.data.Data[i, j, 1] = B2.data.Data[i2, j, 1];
						bOut.data.Data[i, j, 2] = B2.data.Data[i2, j, 2];
					}
				}
			}
			else
			{
				totW = B1Ht - bTot;

				for (int i = bTot; i < B1Ht; i++)
				{
					w1 = B1Ht - i - 1;
					w2 = i - bTot + 1;
					i2 = i - bTot;

					for (int j = 0; j < bOut.width; j++)
					{
						bOut.data.Data[i, j, 0] = (byte)((w1 * B1.data.Data[i, j, 0] + w2 * B2.data.Data[i2, j, 0]) / totW);
						bOut.data.Data[i, j, 1] = (byte)((w1 * B1.data.Data[i, j, 1] + w2 * B2.data.Data[i2, j, 1]) / totW);
						bOut.data.Data[i, j, 2] = (byte)((w1 * B1.data.Data[i, j, 2] + w2 * B2.data.Data[i2, j, 2]) / totW);
					}
				}

				if (bTot < B1Ht)
				{
					for (int i = B1Ht; i < bOut.height; i++)
					{
						i2 = i - bTot;
						for (int j = 0; j < bOut.width; j++)
						{
							bOut.data.Data[i, j, 0] = B2.data.Data[i2, j, 0];
							bOut.data.Data[i, j, 1] = B2.data.Data[i2, j, 1];
							bOut.data.Data[i, j, 2] = B2.data.Data[i2, j, 2];
						}
					}
				}
				else
				{
					for (int i = B1Ht; i < bOut.height; i++)
					{
						i2 = i - B1Ht;
						for (int j = 0; j < bOut.width; j++)
						{
							bOut.data.Data[i, j, 0] = B2.data.Data[i2, j, 0];
							bOut.data.Data[i, j, 1] = B2.data.Data[i2, j, 1];
							bOut.data.Data[i, j, 2] = B2.data.Data[i2, j, 2];
						}
					}
				}
			}

			return bOut;
		}

		// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void handle_PPM_single_threaded_new()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			int transEst = 0,
				pyramidLevel = 3;

			double oldTrans = 0,
					xTrans = 0,
					total_trans = 0;

			int fStart = 2;

			Bitmap_Mine b1 = null, b2 = null;
			ColorBitmap_Mine cB = null;

			for (int i = 0; i < fStart; i++)
			{
				try
				{
					b1 = image_grey_undistorted.Take();
					cB = image_color_undistorted.Take();
				}
				catch (InvalidOperationException e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine("--> Error on ---- Special ---- 'Take()'");
					break;
				}
			}

			if (b1 == null || cB == null)
				return;

			//var cBOut = image_color_undistorted[0];
			var cBOut = new ColorBitmap_Mine(cB.data.Width, cB.data.Height * 400);

			cBOut.height = 0;

			int f = fStart;

			//for (int f = fStart + 1; f < image_grey_undistorted.Length; f++)
			while (!image_grey_undistorted.IsCompleted)
			{
				f++;

				try
				{
					//var b2 = image_grey_undistorted[f];
					b2 = image_grey_undistorted.Take();
					cB = image_color_undistorted.Take();
				}
				catch (InvalidOperationException e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine("Error on 'Take()'");
					break;
				}

				xTrans = LSq_computeXTrans(b2, b1, f, transEst, pyramidLevel);
				//xTrans = 2;

				

				//Console.WriteLine("{0}: {1:0.00}", f, xTrans);

				if (f % 200 == 0)
					Console.WriteLine("\t\t\t\t\t\t Frames stitched {0}: ", f);

				if (xTrans >= 2 && xTrans < cB.data.Height-1)
				{
					Console.WriteLine(xTrans);

					var bTmp = b2;
					b2 = b1;
					b1 = bTmp;

					constructVerMosaic(cBOut, cB, xTrans, oldTrans, ref cBOut);

					oldTrans += (int)xTrans;
					transEst = (int)(0.6 * xTrans);

					if ((int)xTrans > 0)
						total_trans += xTrans;
				}
				else
					transEst = 0;
			}

			image_color_undistorted.Dispose();
			image_grey_undistorted.Dispose();

			cBOut.data.ROI = new Rectangle(cBOut.xstart, cBOut.ystart, cBOut.width, cBOut.height);
			//cBOut.data.Save("_stitched_image_new.jpg");

			stitched_image = cBOut;
			stitch_done = true;

			sw.Stop();
			Console.WriteLine("'handle_PPM_single_threaded' time = {0}", sw.ElapsedMilliseconds);
		}

		public Bitmap StitchVideo()
		{


			// Adds images to a thread-safe queue, by using a seperate thread
			// TODO 9: Start a new thread
			Task.Factory.StartNew(() => {
				decode_all_frames();
			});

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// TODO 9: Start a new thread
			Task.Factory.StartNew(() => {
				//undistort_all_frames();
				undistort_all_frames_parallel();
			});

			//Thread.Sleep(500);

			handle_PPM_single_threaded_new();

			// todo 9: Wait on tasks
			//task.Wait();

			sw.Stop();
			Console.WriteLine("Process whole video time = {0}", sw.ElapsedMilliseconds);

			return stitched_image.data.ToBitmap();
		}

		public void AddImage(Bitmap img, bool doneAdding)
		{
			if (!doneAdding)
			{
				image_queue.Add(img);
			}
			else
			{
				image_queue.CompleteAdding();
			}
		}

		public void ProcessStream()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			// TODO 9: Start a new thread
			Task.Factory.StartNew(() => {
				//undistort_all_frames();
				undistort_all_frames_parallel();
			});

			//Thread.Sleep(500);
			Task.Factory.StartNew(() => {
				handle_PPM_single_threaded_new();
			});

			sw.Stop();
			//Console.WriteLine("Process whole video time = {0}", sw.ElapsedMilliseconds);
		}

		public Bitmap GetStitchResult()
		{
			while (!stitch_done)
				Thread.Sleep(50);

			return stitched_image.data.ToBitmap();
		}

	}// end class 'Stitch_Generator'
}
