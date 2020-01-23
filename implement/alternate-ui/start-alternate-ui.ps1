
# Using the 'debug' option will lead to more processing time on the web client. Disable it when inspection of the state model is not needed.
# ."C:\replace-this-the-path-on-your-system\PersistentProcess.WebHost.exe" build-config --frontend-web-elm-make-appendix="--debug" --output="./build-output/app-config.zip"
."C:\replace-this-the-path-on-your-system\PersistentProcess.WebHost.exe" build-config --output="./build-output/app-config.zip"

."C:\replace-this-the-path-on-your-system\PersistentProcess.WebHost.exe" start-server --webAppConfigurationFilePath="./build-output/app-config.zip" --processStoreDirectoryPath="./runtime-artifacts/process-store"

