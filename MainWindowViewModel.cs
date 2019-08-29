using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace YoloMarkNet
{
    public class Image : Notifier
    {
        private bool _isSelected;
        public string Path { get; set; }
        public ImageSource Thumb { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class BoundingBox : Notifier
    {
        private Rect _rect;

        public BoundingBox(Rect rect, Class c)
        {
            _rect = rect;
            Class = c;
        }

        public Class Class { get; }

        public double X
        {
            get => _rect.X;
            set
            {
                _rect.X = value;
                NotifyPropertyChanged();
            }
        }

        public double Y
        {
            get => _rect.Y;
            set
            {
                _rect.Y = value;
                NotifyPropertyChanged();
            }
        }

        public double Width
        {
            get => _rect.Width;
            set
            {
                _rect.Width = value;
                NotifyPropertyChanged();
            }
        }

        public double Height
        {
            get => _rect.Height;
            set
            {
                _rect.Height = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class Class
    {
        public Class(string desc, Brush color)
        {
            ClassDescriptor = desc;
            Color = color;
        }

        private string ClassDescriptor { get; }
        private Brush Color { get; }
    }

    public class MainWindowViewModel : Notifier
    {
        private static readonly Random seededRandom = new Random(694201337);
        private Rect _ghost;
        private bool _isMouseDown;
        private Class _selectedClass;

        private ImageSource _selectedImageSource;

        private Point mouseDownLocation;

        public MainWindowViewModel()
        {
            Directory.CreateDirectory("data\\img");

            //Fix design time errors
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            Classes = File.ReadAllText("data\\obj.names")
                .Split('\n')
                .Select(f => new Class(f.Trim(), GetPseudorandomBrush()))
                .ToList();

            Images = Directory.GetFiles("data\\img", "*.jpg")
                .Select(
                    f => new Image
                    {
                        Path = f,
                        Thumb = LoadImage(f, true)
                    })
                .ToList();

            SelectedImage = Images.FirstOrDefault();
            if (SelectedImage == null)
            {
                MessageBox.Show("No images found, please place images into data/img/");
                Application.Current.MainWindow.Close();
                return;
            }

            var data = Images.Aggregate("", (current, img) => current + $"{img.Path}{Environment.NewLine}");
            File.WriteAllText("data\\train.txt", data);


            SelectedClass = Classes.FirstOrDefault();
            if (SelectedClass != null) return;
            MessageBox.Show("No classes found, please enter classes into data/obj.names");
            Application.Current.MainWindow.Close();
        }

        public Rect Ghost
        {
            get => _ghost;
            set
            {
                _ghost = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsMouseDown
        {
            get => _isMouseDown;
            set
            {
                _isMouseDown = value;
                NotifyPropertyChanged();
            }
        }

        public IList<Class> Classes { get; }

        public IList<Image> Images { get; }

        public IList<Brush> Colors { get; }

        public ObservableCollection<BoundingBox> BoundingBoxes { get; } = new ObservableCollection<BoundingBox>();

        public bool IsLastImage => SelectedImage == Images.Last();

        private Image SelectedImage
        {
            get { return Images.FirstOrDefault(f => f.IsSelected); }
            set
            {
                SelectedImageSource = null;
                foreach (var image in Images)
                    if (image == value)
                    {
                        image.IsSelected = true;
                        SelectedImageSource = LoadImage(image.Path);
                    }
                    else
                        image.IsSelected = false;

                BoundingBoxes.Clear();
                var path = SelectedImage.Path.Substring(0, SelectedImage.Path.Length - 4) + ".txt";
                if (File.Exists(path))
                {
                    var boundingBoxes = File.ReadAllLines(path);
                    foreach (var bb in boundingBoxes)
                    {
                        var split = bb.Split(' ');
                        var x = double.Parse(split[1]) * SelectedImageSource.Width;
                        var y = double.Parse(split[2]) * SelectedImageSource.Height;
                        var w = double.Parse(split[3]) * SelectedImageSource.Width;
                        var h = double.Parse(split[4]) * SelectedImageSource.Height;
                        var classIdx = int.Parse(split[0]);
                        var rect = new Rect(x, y, w, h);
                        BoundingBoxes.Add(new BoundingBox(rect, Classes[classIdx]));
                    }
                }

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsLastImage));
            }
        }

        public ImageSource SelectedImageSource
        {
            get => _selectedImageSource;
            set
            {
                _selectedImageSource = value;
                NotifyPropertyChanged();
            }
        }

        public Class SelectedClass
        {
            get => _selectedClass;
            set
            {
                _selectedClass = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand SelectImageCommand => new Command(
            p =>
            {
                SaveCurrentImage();
                SelectedImage = (Image) p;
            });

        public ICommand DeleteBoundingBoxCommand => new Command(p => { BoundingBoxes.Remove((BoundingBox) p); });

        public ICommand SaveCommand => new Command(
            _ =>
            {
                SaveCurrentImage();
                var currentImgIdx = Images.IndexOf(SelectedImage);
                if (currentImgIdx < (Images.Count - 1))
                    SelectedImage = Images[currentImgIdx + 1];
                else
                    Application.Current.MainWindow.Close();
            });

        private Brush GetPseudorandomBrush()
        {
            var brushes = typeof(Brushes).GetProperties();
            var random = seededRandom.Next(brushes.Length);
            return (Brush) brushes[random].GetValue(null, null);
        }

        private ImageSource LoadImage(string path, bool thumb = false)
        {
            var src = new BitmapImage();
            using (var stream = File.OpenRead(path))
            {
                src.BeginInit();
                src.StreamSource = stream;
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
            }

            if (!thumb) return src;
            var scaleX = 128f / src.PixelWidth;
            var scaleY = 128f / src.PixelHeight;
            return new TransformedBitmap(src, new ScaleTransform(scaleX, scaleY));
        }

        public void MouseMove(Point point)
        {
            Ghost = new Rect(mouseDownLocation, point);
        }

        public void MouseDown(Point point)
        {
            IsMouseDown = true;
            mouseDownLocation = point;
        }

        public void MouseUp(Point point)
        {
            if (IsMouseDown)
            {
                var rect = new Rect(mouseDownLocation, point);
                BoundingBoxes.Add(new BoundingBox(rect, SelectedClass));
            }

            IsMouseDown = false;
        }

        public void MouseLeave()
        {
            IsMouseDown = false;
        }

        private void SaveCurrentImage()
        {
            var path = SelectedImage.Path.Substring(0, SelectedImage.Path.Length - 4) + ".txt";
            var data = "";
            foreach (var bb in BoundingBoxes)
            {
                var classIndex = Classes.IndexOf(bb.Class);
                var scaledX = bb.X / SelectedImageSource.Width;
                var scaledY = bb.Y / SelectedImageSource.Height;
                var scaledWidth = bb.Width / SelectedImageSource.Width;
                var scaledHeight = bb.Height / SelectedImageSource.Height;
                data += $"{classIndex} {scaledX} {scaledY} {scaledWidth} {scaledHeight}{Environment.NewLine}";
            }

            File.WriteAllText(path, data);
        }
    }
}