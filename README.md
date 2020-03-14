# SortZilla

## Summary

Sorts a very large data set of images to different folders by their labels

<a href="https://imgur.com/tHiQhVD"><img src="https://i.imgur.com/tHiQhVD.png" title="source: imgur.com" /></a><br><br>
*SortZilla UI (running)*
<a href="https://imgur.com/I2I9Ctw"><img src="https://i.imgur.com/I2I9Ctw.png" title="source: imgur.com" /></a><br>
*SortZilla UI (stopped)*

# Technologies
- All written using **C#**
- **Threading** done with **BackgroundWorker** in order to avoid freezing UI during sorting operation
- UI designed in **WPF/XAML**
# Functionalities
- The software sorts a very large data set of different images (using their assigned labels/tags) to different folders, to substantially decrease the time that would have been required to manually sort the images
- Output would be used for a document classifier using image processing to identify the type of a scanned document

## Important
- Due to the very large image data set required to run the algorithm (30+GB), which is not uploaded, it is not possible to run the application

<!--stackedit_data:
eyJoaXN0b3J5IjpbLTM2ODUzMjEyMCwtMTkxNzU3OTIzMV19
-->