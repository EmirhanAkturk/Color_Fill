# Color_Fill
* Projede; Object Pooling, Event System, Singleton Class Patterns kullanılmıştır.
* Alanı dolduracak küpler oyun başladığında oluşturularak Object Pooling yapısında saklanmıştır.
* Player cube arkasında veya plane doldurmak için küp oluşturulması gerektiğinde Object Pooling'te saklanan küpler kullanılır.
* FillController, LevelController ve GameManager sınıfları Singleton Class yapısındadır.
* Oyunun oynanması, kazanılıp kaybebilmesi durumu ve UI panellerinin kontrolü Event System yapısı ile yapılmıştır.
* PlayerControl scriptinde karakterin hareketi ve kapalı alan oluşması kontrolü gibi durumlar kontrol edilmiştir.
* Kapalı alan oluştuğunda alanın doldurulması için recursive Boundary Fill  algoritması kullanılmıştır.

Boundary Fill Algoritması kaynak linki: https://www.geeksforgeeks.org/boundary-fill-algorithm/
