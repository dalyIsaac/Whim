# Build the installer.
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" src\whim-install.iss

# Install Whim.
.\src\whim-install.exe
