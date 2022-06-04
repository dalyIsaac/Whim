# Build the installer.
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" .\whim-installer.iss

# Install Whim.
.\bin\whim-install.exe
