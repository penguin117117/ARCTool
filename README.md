# ARCTool
SMG1,SMG2で使用されるarcファイルを解凍、圧縮できます。<br>
Yaz0未対応

## 使い方
arcファイルまたは、フォルダをexeファイルに<br>
ドラッグアンドドロップするだけです。<br>

## 従来のARCToolとの違い
サブフォルダを含むARCファイルを作成できます。<br>
日本語名のフォルダなどを含む深い階層のフォルダも圧縮できます。<br>
従来のARCToolとは、ファイル構造が若干異なります。<br>
一度手持ちのファイルで、どのように変わったか確認してから使用してください。<br>
Yaz0の処理はまだできません。<br>
<br>
XXXXGalaxyDesign.arcを例に解説します。<br>
<br>
従来のARCToolの場合<br>
親フォルダ：XXXXGalaxyDesign<br>
子フォルダ：jmp<br>
孫フォルダ：Debug<br>
孫フォルダ：GeneralPos<br>
孫フォルダ：List<br>
孫フォルダ：MapParts<br>
孫フォルダ：Path<br>
孫フォルダ：Placement<br>
孫フォルダ：Start<br>

今回作ったARCToolの場合<br>
作業フォルダ：XXXXGalaxyDesign←これがファイル名になります(XXXXGalaxyDesign.arc)<br>
※作業フォルダに親フォルダが生成されます。<br>
親フォルダ：Stage<br>
子フォルダ：jmp<br>
孫フォルダ：Debug<br>
孫フォルダ：GeneralPos<br>
孫フォルダ：List<br>
孫フォルダ：MapParts<br>
孫フォルダ：Path<br>
孫フォルダ：Placement<br>
孫フォルダ：Start<br>
