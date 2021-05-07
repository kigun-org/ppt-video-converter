# Converter

Easily convert videos to a PowerPoint friendly format.

Users creating presentations frequently use videos from different sources. These videos are
not always compressed ideally, resulting in unneccessarily large PowerPoint files.

It is a very basic wrapper around HandBrake CLI (https://handbrake.fr/) and converts any given video
to a PowerPoint recommended format. See https://support.microsoft.com/en-us/office/video-and-audio-file-formats-supported-in-powerpoint-d8b12450-26db-4c7b-a5c1-593d3418fb59
for details.

Specifically, the input videos are converted to the following format:

- MP4 container
- H.264 (x264) video codec
- no audio

The default settings of HandBrake are used, please see their documentation for details.

The output video is placed in the same folder as the input video and given a unique name.
The name of the input file is used with the .mp4 extension, if the file does not already exist,
otherwise a number is added to the end of the filename.

## Requirements

The .NET 5 Desktop Runtime is required. You can download it for free for your platform here:
https://dotnet.microsoft.com/download/dotnet/5.0

## Usage instructions

Download and unzip the archive.

You can use the Converter.exe application inside the folder in the following ways:

1) Start the application. Drag and drop any videos you would like to convert to the queue list
   (the initially empty box on the left side of the window). The conversion will start immediately
   and will be able to follow the progress of the operation.

1) Drag and drop any video you would like to convert directly onto the application icon.
   The application will launch and start converting the videos immediately. You can drag
   and drop additional files to the queue list.

You can cancel the currently running job by clicking the 'Cancel' button.

## Copyright

This software is developed by Mihai Tarce (mihai@o3dp.org) and released under the GPL v3 open source license.
Please read the LICENSE file for details.