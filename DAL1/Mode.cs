namespace DAL
{
    public enum WatermarkingMode
    {
        AllToAll = 0,
        OneKeyToAllContainers = 1,
        OneKeyToAllContainersWithNoise,
        OneKeyToAllContainersWithBrightness,
        OneKeyToAllContainersWithContrast,
        OneContainerToAllKeys,
        OneContainerToAllKeysWithNoise,
        OneContainerToAllKeysWithBrightness,
        OneContainerToAllKeysWithContrast,
        Single,
    }
}
