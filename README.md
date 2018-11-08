# SphericalHarmonicLighting
a project which introduction to SphericalHarmonicLighting in Game using Unity

about project data:
I use stanford dragon as model which has 5000 vertecies and 10000 faces.
I use some cubemap as light source.

about project operator:
you can move mouse to rotate light source and press to backspace to change a light source in run-time.

about project Scripts:
coffeeSHIntegrator : these part pre-Compute DiffuseWithShadow-SH  Light tranfer Function Integrator.
InterReflection : these part pre-Compute InterReflection-SH Light tranfer Function Integrator.
LightSHIntegrator : these part pre-Compute Light Source Integrator.
SHRender.cs & SHRender.shader : Run-time Render SH with CPU.
SHRender1.cs & SHRender1.shader: Run-time Render SH with GPU and ps-part.

about SphericalHarmonicLighting:

you can get more information from my blog:

##############################

一个用于介绍球谐光照原理的Unity项目。

数据：
模型使用的斯坦福龙，光源是不同的cubemap。

操作：
你可以移动鼠标来移动光源，按下空格切换一个光源。

脚本：
coffeeSHIntegrator.cs: 用于Diffuse&自阴影效果的光照传输函数的预计算积分
InterReflection.cs:用于内反射效果的光照传输函数的预计算积分
LightSHIntegrator.cs:用于光源的预计算积分
SHRender：在CPU的SH实时渲染
SHRender1：在GPU端的SH实时渲染

关于球谐光照，你可以获取更多的信息从我的博客：
