namespace DAL
{
    public enum WatermarkingMode
    {
        AllToAll = 0,
        OneKeyToAllContainers = 1,
        OneKeyToAllContainersWithNoise = 2,
        OneKeyToAllContainersWithBrightness = 3,
        OneKeyToAllContainersWithContrast = 4,
        OneContainerToAllKeys = 5,
        OneContainerToAllKeysWithNoise = 6,
        OneContainerToAllKeysWithBrightness = 7,
        OneContainerToAllKeysWithContrast = 8,
        Single = 9,
    }
}
