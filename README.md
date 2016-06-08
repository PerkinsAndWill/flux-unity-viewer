# Unity-Flux Example Project

## User information

### Controls

  * WASD or arrow keys to move around
  * Move mouse or trackpad to look around (after clicking "use mouse")
  * Space to jump — you can keep jumping in the air to get higher
  * Click the left mouse button to shoot a frisbee

### Options

  * If your loaded geometry is too large, you can change the size it is rendered
  at by changing the 'size' field.

## Developer information

### Getting started

Whether you're on mac, windows, or linux, the steps should be the same:
    1. install node.js
    2. run `npm install` inside this directory
    $ node server.js

### Launching the unity project

Install Unity, then open the "unity-project" folder as a project in unity.
You can edit things inside the unity editor and they will show up in the webgl
application after you rebuild the unity files.

To rebuild the unity files in the webgl client, select: File > Build settings...
and then click "Build." Select the "unity-build-target" folder, replacing the
folder 'webgl.' The .ejs templates in views/ will automatically load your new
version of the unity project.

### More features

* If you want to see tooltips on an element, you can change the script to use
the CreateGeometryAndBimInfo method instead of CreateGeometry. This method
takes a flux data key in the shape of an array with two items, the first being
the geometry and the second being your bim data.

* If you want to shoot frisbees, change the static variable
PlayerController.FrisbeesOff to false. You can do this with SendMessage
in the console on the webapp if you want to.

### Project notes

Please find the project notes here.
https://docs.google.com/document/d/1zhflOlrxRYR-xru0biE-voybzLv_zLDCi_2gUcgNGks/edit#heading=h.zg4o77vrpkc4