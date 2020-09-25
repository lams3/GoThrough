# GoThrough

> An Unity plugin that enables it's users to easily add transformative portals to their scenes.

![A House of Mirrors made using GoThrough](https://i.imgur.com/mtRk02O.gif)

## Introduction

GoThrough is an Unity Plugin developed to enable the usage of transformative portals in the Universal Render Pipeline.

The tool supports an arbitrary number of portals that can be recursivelly rendered up to a maximum recursion depth.

## Installation

1. Install **Unity 2019.4LTS**;
2. Create a new project using the **Universal Render Pipeline (URP)**;
3. Download [GoThrough's Unity Package](https://github.com/lams3/GoThrough/releases/tag/v1.0.0);
4. Import GoThrough's Unity Package into your project.

## Usage

### Setup the PortalManager

The first step to have GoThrough Portals working in your scene, is to place an instance of the PortalManager prefab on it. The prefab can be found at the GoThrough/Prefabs folder.

![Drag the PortalManager prefab.](https://i.imgur.com/3Myntwg.png)

### Setup a PortalRenderer

To properly render Portals, a camera must have an PortalRenderer component attached to it. This component is responsible for managing all the resources needed to correctly render Portals. Recursion depth and the maximum number of textures allocated can be changed through the inspector.

![Add a PortalRenderer component to the Camera.](https://i.imgur.com/QZh1mLH.png)
![PortalRenderer component added.](https://i.imgur.com/uDDCi5Q.png)

### Setup Travellers

In order to travel through Portals, GameObjects need to be properly configured as Travellers. The Player prefab in GoThroughSamples/Common will be used as an example. 

First, any graphical part (meshes) of the GameObject should be moved to a separate child object. The materials in those meshes should also use one of the shaders in the GoThrough/Shaders/Traveller folder.

![The Player prefab with the graphics object selected.](https://i.imgur.com/HjtsYB9.png)

Then a Traveller component must be added to the GameObject with a reference to it's graphics child previously created.

![The Player prefab.](https://i.imgur.com/z3qBvf4.png)

GoThrough's Traveller Component doesn't currently support rigged meshes as part of the graphics GameObject.

### Setup Portals

Portals can be placed using the Portal prefab in GoThrough/Prefabs. After placing a Portal in the scene, it's destination should be referenced through the inspector. A Portal's size can be adjusted by scaling it's screen GameObject.

![The Portal prefab.](https://i.imgur.com/aHB69iG.png)

## Limitations

- Only works in the Universal Render Pipeline.
- Only tested in Unity 2019.4LTS.
- Traveller graphics does not support rigged meshes.

## License

GoThrough is freely available for free non-commercial use, and may be redistributed under these conditions. Please, see [LICENSE](./LICENSE) for further details. Interested in a commercial license? Contact [Voxar Labs](https://www.cin.ufpe.br/~voxarlabs) at voxarlabs@cin.ufpe.br.

## Authors

- [Luca Ananias Moraes da Silva](https://lams3.github.io) (lams3@cin.ufpe.br)
- [Voxar Labs](https://www.cin.ufpe.br/~voxarlabs) (voxarlabs@cin.ufpe.br)