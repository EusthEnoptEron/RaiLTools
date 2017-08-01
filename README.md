# RaiLTools

Quick 'n dirty .NET library for working with Liar-soft's and raiL-soft's projects. Mainly tested with Albatross Log.

![Saya meets Albatross](https://raw.githubusercontent.com/EusthEnoptEron/RaiLTools/master/Examples/albatross_edited.jpg "You can do cool stuff.")

## Usage

A few notes for using the library.

### Archive Files (.xfl)

```csharp
var archive = XflArchive.FromFile("archive.xfl");
archive.CreateEntry("newFile.wcg", File.ReadAllBytes("my.wcg"));
archive.Save("archive2.xfl");
archive.ExtractToDirectory("archive_extracted");)
```

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

#### Reading from Scenario Files

It is possible to a certain degree to read the commands in a scenario file using the `CommandTranslator` or the `CommandTokenizer`.

```csharp
var scenario = GscFile.FromFile("my_text.gsc");
var translator = new CommandTranslator(scenario);

foreach(var command in translator.GetCommands()) {
  Console.WriteLine(command);
}
```

The `CommandTranslator` will try to map the tokens emitted by the tokenizer to subclasses of `ICommand`. If it fails, it will simply create `UnknownCommand` instances. Currently mapped:

- Sprite images
- Background images
- Text
- Goto, AND, Assign and some more operations
- Transitions


### Image Files (.wcg)

Only 32-Bit files are supported.

To turn WCG files into any sort of file you want:

```csharp
using (var wcg = WcgImage.FromFile("my_image.wcg"))
using (var img = wcg.ToImage())
{
    img.Save("my_image.png");
}
```

The images can be turned back in a similar manner:

```csharp
using (var img = Image.FromFile("my_image.png"))
using (var wcg = WcgImage.FromImage(img))
{
    wcg.Save("my_image.wcg");
}
```

Note that the generated WCG files are anything but optimized.


### Canvas Files (.lwg)

Putting aside whether or not they can be called "canvas" files, they store a list of images (WCG) with a position (x, y).

You can load them like so:

```csharp
var canvas = LwgCanvas.FromFile("my_canvas.lwg");
foreach(var image in canvas.Entries)
{
    Console.WriteLine("IMG: {0} [x={1}, y={2}]", item.Path, item.X, item.Y);
}
```

You can also populate a (new) LWG file.

```csharp
var canvas = new LwgCanvas("my_canvas.lwg");
canvas.ReplaceImage("content_wcg", WcgFile.FromFile("content_wcg.wcg"), 0, 0);
canvas.Save("my_new_canvas.lwg");
```

The class also has a few helper methods that make it easy to extract and import entire folders.
