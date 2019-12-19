
."C:\replace-this-the-path-on-your-system\PersistentProcess.WebHost.exe" build-config --frontend-web-elm-make-appendix="--debug" --output="./build-output/app-config.zip"

."C:\replace-this-the-path-on-your-system\PersistentProcess.WebHost.exe" "start-server" --webAppConfigurationFilePath="./build-output/app-config.zip" --processStoreDirectoryPath="./runtime-artifacts/process-store"

