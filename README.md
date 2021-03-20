# OPNA-VirtualMultiTrackRML
OPNAのリズム音源で、楽器毎にトラックを分けて打ち込む為のラッパーRMLです。  
現在は、[MUCOM88](https://onitama.tv/mucom88/) への変換にしか対応していませんが、もし他のMMLツールでも使ってみたいというご要望があれば、前向きに検討します。  

実行ファイルのダウンロードは[こちら](https://github.com/DM-88mkII/OPNA-VirtualMultiTrackRML/blob/main/OPNA-VirtualMultiTrackRML/bin/Release/OPNA-VirtualMultiTrackRML.exe)  

<br>

# 使い方
~~~
OPNA-VirtualMultiTrackRML.exe 入力ファイル

エラーが無ければ、変換後の文字列がクリップボードにコピーされます。
~~~

<br>

# 書式
[MUCOM88](https://onitama.tv/mucom88/) に似せた書式になっています。  
~~~
;設定
#VOLUME_UPDOWN [)(|()]

;マクロ定義
# *n{}

;トラック定義
B BDトラック
S SDトラック
C TOPトラック
H HHトラック
T TOMトラック
R RIMトラック
~~~
変換後は、Gパートに纏められます。  

<br>

# MMLコマンド
[MUCOM88](https://onitama.tv/mucom88/) の通常パートに似せたコマンドになっています。  
~~~
音符
    発音      c
    休符      r

コマンド
    音量      ) ( V v
    音長      C l q . & ^ %
    リピート  [ ] /
    テンポ    t T
    マクロ    *
    その他    ; : ! |

pコマンド
    pM 出力なし
    pL 左出力
    pR 右出力
    pC 中央出力

yコマンド
    yRTL,パラメータ    リズム音源全体の音量設定
    パラメータの頭文字に $ を付けると、16進数で指定できます。
~~~

* 備考
  * Lコマンドは、Bトラックの指定が有効となり、他のトラックは強制的にループが適用されます。
  * tコマンドとTコマンドは、全トラックに掛かります。

<br>

# その他

変換後のMMLは、yコマンドが膨大な量となる為、演奏バッファの少ないMMLツールでは、利用が困難な場合があります。  

MUCOM88 で利用される場合は、driver を次のものに設定することを推奨します。
* mucom88EM
* mucomDotNET
