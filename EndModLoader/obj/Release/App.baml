   M S B A M L   `   `   `  ���� I  ETEiNRandomizer, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:TEiNRandomizerTEiNRandomizer      TEiNRandomizer.App   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     .localclr-namespace:TEiNRandomizer       �� 	Resources  &    TEiNRandomizer.SettingsDisplayer   SettingsDisplayer  AdvancedSettings �� 
TargetType �� Property �� Value �� 	Converter �� Triggers   TEiNRandomizer.AppState   InGame �� Value �� Binding  	TitleText  TabTitleText  DescriptionText  
BottomText  PoolIndicator  SWFIndicator  PathText	   IncorrectPath
 �� ContentStringFormat 	 PoolListStyle 
 PoolCatListStyle  FreezeSettings  FolderSelectButton  PlayModButton   NoModsFound   NoModSelected   ReadyToPlay  RefreshButton  UnloadButton  PieceListTemplate �� Orientation � ColumnDefinitions d� 	IsChecked  PoolListTemplate  ShadersListTemplate  PoolCatListTemplate � RowDefinitions  ListBoxItemStyle��%Z  &	        &	      7   &	 �     &	 �     &	 d    &	 �    &	 s    &	     &	 �    &		     &	
 �    &	 �    &	     &	 �    &	 �
    &	 \    7   &	 	    7  7  &	 4    7  7  &	 _    &	 �    7  7  7 	 7     �� " ��.���  � y� �� *ShowAdvancedSettings+8    �� # Z  �� *
AppState+.a���  � {� .  �� " ��.���  � ��$ 12(���  � ��$ Bottom=���  � ��$ CharacterEllipsis=��� " ��.���  � ��$ 15(���  �@��Padding$ 5q���  � ��$ Bottom=���  � ��$ CharacterEllipsis=��� " ��.���  � ��$ 11(���  � ��
 �������  � ��$ Bottom=���  � ��$ 5,0,0,0q���  � ��$ CharacterEllipsis=��� " ��.���  � ��$ 11(���  � ��
 �������  � ��$ Bottom=���  � ��$ CharacterEllipsis=��� " ��.���  � ��$ 11(���  � ��
 �   ���  � ��
 ������  � ��$ Bottom=���  � ��$ 60����  �@��Padding$ 4,0,4,0q���  �@��TextAlignment$ Center=���  � ��$ Center=� ��  �� *Active+	 False.a���  � ��$ Disabled����  � ��
 ��p����  �� *Active+ True.a���  � ��$ Enabled����  � ��
 ������ " ��.���  � ��$ 11(���  � ��
 �   ���  � ��
 �������  � ��$ Bottom=���  � ��$ 15����  �@��Padding$ 4,0,4,0q���  �@��TextAlignment$ Center=��� " ��.���  � ��$ CharacterEllipsis=���  � {� . ��  � ��$ 0,0,0,0q���  � �� �� *EndIsNighPath+ �� # Z	  �� *
AppState+.a���  � ��
 �  ����  �@�Cursor$
 Help����  �@�ToolTip _� $
 No {0} found in path.������ *	ExeName+�� " ��.���  � ��$ Stretch=���  � ��$ Disabled=���  � ��$ Disabled=� �� # Z  �� *
AppState+.a���  � {� .  ��  �� *
Visibile+	 False.a���  � y�$ 	Collapsed=��� " ��.���  � ��$ Stretch=���  � ��$ Disabled=� �� # Z  �� *
AppState+.a���  � {� .  ��  �� *
Visibile+	 False.a���  � y�$ 	Collapsed=��� " �� �� # Z  �� *
AppState+.a���  � {� .  �� " ��.���  � ��$ Set Path����  � ��$ Right=���  � {� . ��  � ��$ 0,0,0,0q���  � ��$ 90�� �� # Z  �� *
AppState+.a���  � {� .  �� " ��.���  � ��$
 Left=���  � ��$ 0,10,0,0q���  � ��$	 110�� �� # Z	  �� *
AppState+.a���  � ��$ Incorrect Path����  � {� .  �� # Z  �� *
AppState+.a���  � ��$ No Pools Found����  � {� .  �� # Z  �� *
AppState+.a���  � ��$ No Pools Enabled����  � {� .  �� # Z  �� *
AppState+.a���  � ��$ 	Randomize����  � {� . �� # Z  �� *
AppState+.a���  � ��$ In Game����  � {� .  �� " ��.���  � ��$
 Left=���  � ��$ 0,10,0,0q���  � ��$	 110�� �� # Z	  �� *
AppState+.a���  � {� .  �� # Z  �� *
AppState+.a���  � {� .  �� # Z  �� *
AppState+.a���  � {� .  �� # Z  �� *
AppState+.a���  � {� .  �� # Z  �� *
AppState+.a���  � {� . �� " ��.���  � ��$
 Left=���  � ��$ 0,10,0,0q���  � ��$	 110����  � y� �� *RSettings.ManualLoad+8    �� # Z	  �� *
AppState+.a���  � {� .  �� # Z  �� *
AppState+.a���  � {� .  �� # Z  �� *
AppState+.a���  � {� .  �� # Z  �� *
AppState+.a���  � {� .  �� # Z  �� *
AppState+.a���  � {� . �� .R��� $��0.2,5,0.2,5q�$ Vertical=�.+��  �� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��8��  $��10,0,0,0q����� *Name+�� $��1��8�� ���� *	NumPieces+�� $��2��$��Enabled��$��Right=� �� *Active+�� $��2.5, 5, 2.5, 0q��� .R��� $��0.2,5,0.2,5q�$ Vertical=�.+��  �� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��8��  $��10,0,0,0q����� *Name+�� $��1��8�� ���� *	NumLevels+�� $��2��$��Enabled��$��Right=� �� *Active+�� $��2.5, 5, 2.5, 0q��� .R��� ���� *Name+ �� *	Enabled+�� PoolCatItem��.R��� -   |�.  .+��  �� $��20 ��� $
��auto �.+��� $��0��
�������.+��  �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��5,2,0,0q� �� *	Enabled+�� $��1��$��Center=�$��11(�$��Segoe UI Semibold)�$
��Left=�$��5,2,0,0q����� *Name+�� $��2��$��10,0,0,0q�8��  ���� *Author+�� $��PoolList��-   $��1��8�� 8�� #���  ���� *Pools+y��� *	Enabled+8  