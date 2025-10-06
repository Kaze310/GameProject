GameProject 使用说明

一、怎么打开项目

打开链接：https://github.com/Kaze310/GameProject

点绿色的 Code 按钮 → Download ZIP

解压到你喜欢的地方，比如桌面/GameProject

打开 Unity Hub → Add project from disk → 选刚解压的文件夹

等 Unity 把资源导完（第一次会有点慢）

二、我们用的 Unity 版本

统一版本：Unity 6000.2.6f2

这个版本在 Windows 和 macOS 上都稳定能跑，别换别动。

如果 Unity 提示要升级或降级，果断点 “Cancel”。

三、项目里都有啥

打开项目后会看到这些：

Assets
Packages
ProjectSettings
.gitignore

不用找 Library，那是 Unity 自动生成的缓存文件夹，没上传也不需要上传。

四、素材该放哪

在 Assets 下面新建一个文件夹叫 Artwork（如果已经有就用它）。
建议分几个子文件夹：

Assets/Artwork/Textures（贴图）
Assets/Artwork/UI（UI 素材）
Assets/Artwork/Models（模型）
Assets/Artwork/Animations（动画）
Assets/Artwork/ToProgram（要发给程序的导出文件放这里）

命名随意但要清楚：
UI_button_play_v1.png
character_idle_loop.psd

五、发素材给程序的方法

把改过的文件放进 Assets/Artwork/ToProgram

压缩这个文件夹，命名成 ToProgram_你的名字.zip

发给程序

千万别把整个项目或 Library 发上来！！！！

六、不要乱动的东西

Library（Unity 缓存，会自己重建）

Packages（程序员用的依赖配置）

ProjectSettings（工程设置）

.gitignore（Git 规则）

.git（隐藏文件夹）

别人的场景、脚本

如果真要改这些，先问一声。

七、打不开或报错怎么办

用 Unity 6000.2.6f2 打开

等导入完，别着急点什么

如果还是报错，关掉 Unity，删除本地的 Library 文件夹，再用 Unity Hub 打开一遍

不要惊慌，Unity 重建 Library 很慢但正常

八、你可以放心做的事

在 Assets/Artwork 里加素材、改图、调 UI

新建自己的子文件夹收拾资源

看场景布局（别保存）

九、你暂时别做的事

不改别人做的场景

不动代码、不动 ProjectSettings

不上传整个项目

不在 Play 模式里点保存

十、简单回顾

打开版本：Unity 6000.2.6f2

素材都放 Assets/Artwork 里

发素材压缩 ToProgram 给程序

不要动 Library / Packages / ProjectSettings

如果出错，先删 Library 再开

有任何奇怪的报错或资源失踪，先喝口水，再dd我
