# Face-in-the-Wall

[Demo Video](https://www.youtube.com/watch?v=wgD0xDr0xRc)

## System Requirements

- Linux
- Unity 2019.2.6f1
- A CUDA-supported graphics card
- An RGB camera
- anaconda

## Install

```bash
git clone https://github.com/gerwang/Face-in-the-Wall.git --recursive
conda create -n faceinwall python=3.7
conda activate faceinwall
pip install face_alignment opencv-python openmesh
```
- Copy the absolute path of **python executable** to `pythonFileName` in `./Assets/Scripts/FaceCaptureSDK.cs`.
- Copy the absolute path of `2DASL/test_codes`  to `pythonWorkingDir` in `./Assets/Scripts/FaceCaptureSDK.cs`.

## Acknowledgements

The face tracking is based on [2DASL](https://github.com/XgTu/2DASL). The landmark detection is based on [face-alignment](https://github.com/1adrianb/face-alignment).
