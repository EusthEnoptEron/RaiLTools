# RaiLTools

Quick 'n dirty .NET library for working with Liar-soft's and raiL-soft's projects. Only tested with Albatross Log.

## Usage

A few notes for using the library.

### Archive Files (.xfl)

TODO

### Scenario Files (.gsc)

Scenario files can be loaded like so:

```csharp
var transFile = TransFile.FromGSC("my_text.gsc");
transFile.Save("my_text.txt");
```

Edit the stuff and load it back using:

```csharp
var gscFile = TransFile.FromFile("my_text.txt").ToGSC("my_text.gsc");
gscFile.Save("my_new_text.gsc");
```

### Image Files (.wcg)

Only 32-Bit files are supported.

To turn WCG files into any sort of file you want:

```csharp
using (var wcg = WCGImage.FromFile("my_image.wcg"))
using (var img = wcg.ToImage())
{
    img.Save("my_image.png");
}
```

The images can be turned back in a similar manner:

```csharp
using (var img = Image.FromFile("my_image.png"))
using (var wcg = WCGImage.FromImage(img))
{
    wcg.Save("my_image.wcg");
}
```

Note that the generated WCG files are anything but optimized.


### Canvas Files (.lwg)

Putting aside whether or not they can be called "canvas" files, they store a list of images (WCG) with a position (x, y).

You can load them like so:

```csharp
var canvas = LWGCanvas.FromFile("my_canvas.lwg");
foreach(var image in canvas.Values)
{
    Console.WriteLine("IMG: {0} [x={1}, y={2}]", canvas.Path, canvas.X, canvas.Y);
}
```

You can also populate a (new) LWG file.

```csharp
var canvas = new LWGCanvas("my_canvas.lwg");
canvas.AddImage("content_wcg", WCGFile.FromFile("content_wcg.wcg"), 0, 0);
canvas.Save("my_new_canvas.lwg");
```