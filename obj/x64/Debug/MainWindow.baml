   M S B A M L   `   `   `  ����K  GHDFaceBasics-WPF, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]3clr-namespace:Microsoft.Samples.Kinect.HDFaceBasics%Microsoft.Samples.Kinect.HDFaceBasics  5   	   | xSystem.Windows.Controls.DataVisualization.Toolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35�{clr-namespace:System.Windows.Controls.DataVisualization.Charting;assembly=System.Windows.Controls.DataVisualization.Toolkit2System.Windows.Controls.DataVisualization.Charting 5   	   6    0Microsoft.Samples.Kinect.HDFaceBasics.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Elocal3clr-namespace:Microsoft.Samples.Kinect.HDFaceBasics     5   	   �chartingToolkit{clr-namespace:System.Windows.Controls.DataVisualization.Charting;assembly=System.Windows.Controls.DataVisualization.Toolkit     5   	   -   5        $��
mainwindow��5   	     � Title$  HDFace Basics��5   	   $��910.365��5   	   $
��1244��6    � 
ResizeMode$ NoResize=�5	   &   ���	#FFACEEA6 � 	Resources 5         MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value��%�   &	        &	 #     &	 F     '�� i     �� 5   
   $ 	#ff6e6e6e��62   �� 5   
   $ 	#ff52318f��64   �� 5   
   $ 	#ff00BCF2��62   �� 5   
   " ��6   .�5      ��  �@J�SnapsToDevicePixels . 64   5      5      .��5      � -   5        $��
layoutGrid��5      $��10,0,4,0q�6   $	��861��6/   $	��Top=�6<   ���	#FFACEEA6 � ColumnDefinitions 5   
   �� 5      $	��821 �6   �� 5      -   5         �Name$	 one��5      $
��auto �6*   .+�5   
   (  "System.Windows.Controls.WebBrowser  -   5        $
��web1��5      $
��Left=�6"   $��
10,10,0,10q�6=   $	��781��6Q   ��.  $��Hidden=�6�   $��Hidden=�6�   � 5   
   -   5        $��feedback��5   '   $��796,0,-382,-13q�6   ���	#FFACEEA6$��2��6N   .+�5      �� -   5        $
��face��5      $��
4,623,7,-3q�6    $	��227��64   $	��Top=�6A   	 J� RenderTransformOrigin$	 	0.5,1.471&�6Y   �� 5       -   5        $��canvas2��5       $
��Left=�6$   $	��238��6?   $��4,623,0,-16q�6L   $	��Top=�6a   $	��417��6y   �� 5!      -   5        $��canvas1��5!      $
��Left=�6$   $	��248��6?   $��
10,102,0,0q�6L   $	��Top=�6`   $	��402��6x   ���	#FFFDDDD05#      �� 5$      -	   5        $��canvas4��5$      $
��Left=�6$   $��78��6?   $��	10,19,0,0q�6K   $	��Top=�6^   $	��249��6v   ���	#FFFBF5B6.+�5%      �� -
   5        $��startbtn��5%      
 �� Left$
 10��6)   $��45��6:   $	 0.293,-0.263&�6E   $��45��6j   
 �� Top$ 23��6v   �� 5&      -   5        $��colorbtn��5&      $
 69��6)   $��45��6:   $	 0.293,-0.263&�6E   $��45��6j   $ 23��6v   �� 5'      -   5        $��	actionbtn��5'      $	
 130��6*   $��45��6<   $	 0.293,-0.263&�6G   $��45��6l   $ 23��6x   �� 5(      -   5        $��closebtn��5(      $	
 194��6)   $��45��6;   $	 0.293,-0.263&�6F   $��45��6k   $ 23��6w   5)      �� 5*      -   5         ��Name$ canvas3��5*      $
��Left=�6&   $	��263��6A   $��
10,355,0,0q�6N   $	��Top=�6b   $	��402��6z   ���	#FFC5F7FF6�   �� 5+      -   5        $��richTextBox1��5+      $
��Left=�6.   $	��211��6I   $��
23,386,0,0q�6V   $	��Top=�6j   $	��374��6�    �� 
IsReadOnly . . �5,      /� .T�5-      J� 5.      5/      �� 50      ���	#FFFFFBAF$��23,129,25,704q�6+   $��41��6B   �� 51      ���	#FFFFFBAF$��23,170,25,663q�6+   $��41��6B   �� 52      ���	#FFFFFBAF$��23,211,25,622q�6+   $��41��6B   �� 53      ���	#FFFFFBAF$��23,252,25,581q�6+   $��41��6B   �� 54      ���	#FFFFFBAF$��23,293,25,540q�6+   $��41��6B   �� 55      ���	#FFFFAFAF$��15,112,15,532q�6+   �� 56      $
��Wrap=�6   $��Real-time Feedback��6,   $��16(�6F   $
��Bold"�6T   $��23,103,242,750q�6f   ���	#FFFDDDD06~   �� 57      ���	#FF25ECF5$��15,366,15,265q�6+   �� 58      ���	#FFFFFF5C$��15,29,168,782q�6+   �� 59      $
��Wrap=�6   $��Dialogue Content��6,   $��16(�6D   $
��Bold"�6R   $��23,0,261,493q�6d   ���	#FFC5F7FF$��26��6�   $��Bottom=�6�   �� 5:      $
��Wrap=�6   $��Control��6,   $��16(�6;   $
��Bold"�6I   ���	#FFFBF5B6$��23,19,337,834q�6r   $	 -1.134,-1.333&�6�   �� 5;      -   5        $��	speakrate��5;      $
��Left=�6&   $��
78,306,0,0q�6A   $	��Top=�6U   $	 0.5,0.5&�6m   $	��275��6�   �� 5=      -   5        $��Relevant Score��5=      $
��Left=�6.   $��
281,53,0,0q�6I   $	��Top=�6]   $	��106��6u   $��24��6�   5?      �� 5@   
   -   5         ��Name$ image��5@   g   $S��ME:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/Images/record.png��6   $
��Left=�6v   $	��160��6�   $��228,241,-387,0q�6�   $	��Top=�6�   $	��160��6�   $��1��6�   5A      5B      