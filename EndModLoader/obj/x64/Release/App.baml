   M S B A M L   `   `   `  ���� I  ETEiNRandomizer, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:TEiNRandomizerTEiNRandomizer      TEiNRandomizer.App   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     .localclr-namespace:TEiNRandomizer       �� 	Resources  &    TEiNRandomizer.SettingsDisplayer   SettingsDisplayer  	TitleText �� 
TargetType �� Property �� Value  DescriptionText  
BottomText  PoolIndicator �� Triggers �� Binding �� Value  SWFIndicator  PathText   TEiNRandomizer.AppState   IncorrectPath �� ContentStringFormat  PoolListStyle	   InGame  FolderSelectButton 	 PlayModButton
   NoModsFound   NoModSelected   ReadyToPlay 
 PoolListTemplate �� Orientation � ColumnDefinitions d� 	IsChecked��%
  &	        &	      &	 g     &	 �     &	 v    &	     &	 �    &	     &	 �    &		 ]    &	
 t    7  7    �� " ��.���  � ��$ 12(���  � ��$ Bottom=���  � ��$ CharacterEllipsis=��� " ��.���  � ��$ 11(���  � ��
 �������  � ��$ Bottom=���  � ��$ 5,0,0,0q���  � ��$ CharacterEllipsis=��� " ��.���  � ��$ 11(���  � ��
 �������  � ��$ Bottom=���  � ��$ CharacterEllipsis=��� " ��.���  � ��$ 11(���  � ��
 �   ���  � ��
 ������  � ��$ Bottom=���  � ��$ 60����  �@��Padding$ 4,0,4,0q���  �@��TextAlignment$ Center=���  � ��$ Center=� ��  �� *Active+	 False.a���  � ��$ Disabled����  � ��
 ��p����  �� *Active+ True.a���  � ��$ Enabled����  � ��
 ������ " ��.���  � ��$ 11(���  � ��
 �   ���  � ��
 �������  � ��$ Bottom=���  � ��$ 15����  �@��Padding$ 4,0,4,0q���  �@��TextAlignment$ Center=��� " ��.���  � ��$ CharacterEllipsis=���  � {� . ��  � ��$ 0,0,0,0q���  � �� �� *EndIsNighPath+ �� # Z  �� *
AppState+.a���  � ��
 �  ����  �@�Cursor$
 Help����  �@�ToolTip _� $ No {0} found in path.������ *	ExeName+�� " ��.���  � ��$ Stretch=���  � ��$ Disabled=� �� # Z	  �� *
AppState+.a���  � {� .  �� " ��.���  � ��$ Change����  � ��$ Right=���  � {� . ��  � ��$ 0,0,0,0q���  � ��$ 90�� �� # Z	  �� *
AppState+.a���  � {� .  �� " ��.���  � ��$
 Left=���  � ��$ 0,10,0,0q���  � ��$	 110�� �� # Z  �� *
AppState+.a���  � ��$ Incorrect Path����  � {� .  �� # Z
  �� *
AppState+.a���  � ��$ No Mods Found����  � {� .  �� # Z  �� *
AppState+.a���  � ��$ No Mod Selected����  � {� .  �� # Z  �� *
AppState+.a���  � ��$ 	Randomize����  � {� . �� # Z	  �� *
AppState+.a���  � ��$ In Game����  � {� .  �� .R��� $��0.2,5,0.2,5q�$ Vertical=�.+��  �� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��8��  ���� *Name+�� $��1��8�� ���� *	NumLevels+�� $��2��$��Enabled��$��Right=� �� *Active+�� $��2.5, 5, 2.5, 0q�