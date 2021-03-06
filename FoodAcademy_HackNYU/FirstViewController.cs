﻿
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Plugin.Media;


using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using UIKit;

using CoreGraphics;

namespace FoodAcademy_HackNYU
{
	public partial class FirstViewController : UIViewController
	{


		public UIImagePickerController imagePicker = new UIImagePickerController();
		public UIImagePickerController takeImagePicker = new UIImagePickerController();


		Food food = new Food();
		public int foodQuantity = 0;


		//partial void SelectButtonClick(UIButton sender)
		//{
		//	selectImage();
		//}



		async void selectImage()
		{
			var selectedImage = await CrossMedia.Current.PickPhotoAsync();
			SelectedPictureImageView.Image = new UIImage(NSData.FromStream(selectedImage.GetStream()));
			SelectedPictureImageView.Image = MaxResizeImage(SelectedPictureImageView.Image, 250, 250);
			await analyseImage(SelectedPictureImageView.Image.AsJPEG().AsStream());

		}

		async Task analyseImage(Stream imageStream)
		{
			try
			{
				VisionServiceClient visionClient = new VisionServiceClient("c19d4b8bb6c242ea99a8a998195a24f0");
				VisualFeature[] features = { VisualFeature.Tags, VisualFeature.Categories, VisualFeature.Description };
				var analysisResult = await visionClient.AnalyzeImageAsync(imageStream, features.ToList(), null);

				Tag[] list = analysisResult.Tags.ToArray();


				Console.Out.WriteLine("Tags:\n");
				foreach (Tag t in list)
				{
					Console.Out.WriteLine(t.Name);
					Console.Out.WriteLine(t.Confidence);
				}
				Console.Out.WriteLine("Cats:\n");
				foreach (Category c in analysisResult.Categories.ToArray())
				{
					Console.Out.WriteLine(c.Name);
					Console.Out.WriteLine(c.Score);

				}
				AnalysisLabel.Text = string.Empty;

				analysisResult.Description.Tags.ToList().ForEach(tag => AnalysisLabel.Text = AnalysisLabel.Text + tag + "\n");


				//Console.Out.WriteLine(analysisResult.Categories.t);
			
			}
			catch (Microsoft.ProjectOxford.Vision.ClientException ex)
			{
				AnalysisLabel.Text = ex.Error.Message;
			}
		}


		protected FirstViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}


		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);


			//SelectedPictureImageView
			UITapGestureRecognizer tapGesture = new UITapGestureRecognizer(actionSheet);
			SelectedPictureImageView.AddGestureRecognizer(tapGesture);
			//SelectedPictureImageView.Image = food.image;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.


			quantity.Text = foodQuantity.ToString();


			//takePictureView.Layer.CornerRadius = takePictureView.Frame.Size.Width / 2;
			//takePictureView.ClipsToBounds = true;

		}





		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}



		protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
		{


			// determine what was selected, video or image
			bool isImage = false;
			switch (e.Info[UIImagePickerController.MediaType].ToString())
			{
				case "public.image":
					Console.WriteLine("Image selected");
					isImage = true;
					break;
				case "public.video":
					Console.WriteLine("Video selected");
					break;
			}

			// get common info (shared between images and video)
			NSUrl referenceURL = e.Info[new NSString("UIImagePickerControllerReferenceUrl")] as NSUrl;
			if (referenceURL != null)
				Console.WriteLine("Url:" + referenceURL.ToString());

			// if it was an image, get the other image info
			if (isImage)
			{
				// get the original image
				UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;

				if (originalImage != null)
				{
					// do something with the image
					MaxResizeImage(originalImage, 8, 8); // resize image
					Console.WriteLine("got the original image");

					//profileView.Image = null;
					food.image = MaxResizeImage(originalImage, 200, 200); // display



				}
			}
			else { // if it's a video
				   // get video url
				NSUrl mediaURL = e.Info[UIImagePickerController.MediaURL] as NSUrl;
				if (mediaURL != null)
				{
					Console.WriteLine(mediaURL.ToString());
				}
			}

			imagePicker.DismissModalViewController(true);
		}



		void Handle_Canceled(object sender, EventArgs e)
		{

			imagePicker.DismissModalViewController(true);
		}

		async void takePicture()
		{

			await CrossMedia.Current.Initialize();
			var selectedImage = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions());

		


			SelectedPictureImageView.Image = new UIImage(NSData.FromStream(selectedImage.GetStream()));
			 

			SelectedPictureImageView.Image = MaxResizeImage(SelectedPictureImageView.Image, 250, 250);


			await analyseImage(SelectedPictureImageView.Image.AsJPEG().AsStream());

		}

		protected void Camera_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
		{
			UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;

			if (originalImage != null)
			{
				// do something with the image
				Console.WriteLine("got the original image");
				// resize image




				food.image = MaxResizeImage(originalImage, 200, 200); // display
			}
			takeImagePicker.DismissModalViewController(true);
		}

		void Camera_Canceled(object sender, EventArgs e)
		{
			//var imagePicker = new UIImagePickerController();
			takeImagePicker.DismissModalViewController(true);
		}



		public UIImage MaxResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
		{


			Console.WriteLine("Re Size original image");


			var sourceSize = sourceImage.Size;
			var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
			if (maxResizeFactor > 1) return sourceImage;
			var width = maxResizeFactor * sourceSize.Width;
			var height = maxResizeFactor * sourceSize.Height;
			UIGraphics.BeginImageContext(new CGSize(width, height));
			sourceImage.Draw(new CGRect(0, 0, width, height));
			var resultImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			return resultImage;
		}


		//actionSheet
		public void actionSheet()
		{


			//Action Sheet
			UIAlertController actionSheetAlert = UIAlertController.Create("Change Picture", "Select an item from below", UIAlertControllerStyle.ActionSheet);
			actionSheetAlert.AddAction(UIAlertAction.Create("Add Photo", UIAlertActionStyle.Default, (action) => selectImage()));
			actionSheetAlert.AddAction(UIAlertAction.Create("Take Picture", UIAlertActionStyle.Default, (action) => takePicture()));
			actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => Console.WriteLine("Cancel button pressed.")));

			UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
			if (presentationPopover != null)
			{
				presentationPopover.SourceView = this.View;
				presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
			}

			this.PresentViewController(actionSheetAlert, true, null);
		}
	}
}
