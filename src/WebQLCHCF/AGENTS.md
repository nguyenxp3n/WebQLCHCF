# AGENTS.md instructions for WebQLCHCF

Nguoi dung khong code va khong doc code. Nguoi dung chi prompt va kiem tra ket qua bang web view.

Khi lam viec voi du an nay, hay tuan thu:

1. Luon doc file `.md`, `.sql` va source code lien quan truoc khi sua.
2. Chi sua trong pham vi task nguoi dung giao.
3. Khong tu y xoa du lieu, drop table, doi schema database, doi connection string, hoac sua authentication/authorization neu chua giai thich va hoi nguoi dung.
4. Truoc khi sua, noi ngan gon se lam gi.
5. Sau khi sua, tu build/run/test neu co the.
6. Neu co loi, tu doc log va sua tiep trong pham vi task.
7. Sau moi lan hoan thanh, bao cho nguoi dung:
   - Da sua chuc nang gi
   - Nguoi dung can mo trang/URL nao de kiem tra
   - Cac buoc click tren web view
   - Ket qua dung mong doi
   - Co can nguoi dung cap quyen gi khong
8. Khong giai thich code dai dong tru khi nguoi dung hoi.
9. Neu khong chac, hoi nguoi dung truoc thay vi tu doan.
10. Uu tien sua nho, de test, khong lam nhieu chuc nang cung luc.
11. Khi build/test xong luon tat web/dev server.

## Document And Writing Work

Khi viet tai lieu vi du document, Excel, bao cao, README, noi dung bai tap, hoac van ban can giong nguoi viet, dung skill `human-document` neu skill nay co san trong Codex.

Neu nguoi dung yeu cau "humanize", "make this sound more human", "rewrite to avoid AI detection", "make this less AI-sounding", "add a human voice", "write like a person", hoac dan van ban va hoi vi sao doc nhu AI, ap dung quy trinh viet tu nhien sau:

- Doc het input truoc, xac dinh chu de, nguoi doc, giong van, do dai mong muon.
- Khong them thong tin gia de lam van ban cu the hon.
- Neu input khong co moc cu the nao nhu so lieu, ten rieng, ngay thang, cong cu, dia diem, vi du that, hay bao nguoi dung can bo sung moc cu the hoac xac nhan van muon viet tiep.
- Cat cac cum mo dau/khep lai kieu tro ly AI nhu "Here is", "Of course", "I hope this helps", "Let me know if...".
- Tranh tu/cum tu de bi nhan dien la AI: delve, leverage, utilize, robust, comprehensive, streamline, foster, facilitate, pivotal, nuanced, multifaceted, moreover, furthermore, it is important to note, it is worth mentioning, in conclusion, in summary.
- Giam hedge neu khong can thiet. Noi thang y chinh khi thong tin chac.
- Lam cau co do dai thay doi tu nhien. Xen ke cau ngan va cau dai hon khi no thuc su can.
- Uu tien danh tu, dong tu, vi du cu the. Khong dung tinh tu trang tri qua nhieu.
- Tranh cau truc qua tron tru nhu mo bai theo luan diem, ba gach dau dong cho moi thu, ket luan tom tat lai dieu vua noi.
- Tranh cham phay va em dash. Dung dau cham, dau phay, hoac tach cau.
- Dung dau ngoac kep thang trong ngu canh ky thuat va van ban thong thuong.
- Khong dung bullet list neu nguoi dung khong yeu cau va van ban co the viet thanh doan van.
- Neu co mau van cua nguoi dung, uu tien bat chuoc mau do hon la ap quy tac chung.
- Truoc khi tra ket qua, tu quet lai de loai bo cum tu cam, cau qua deu, cau tong ket nghe nhu khau hieu, va cac cau noi chung chung khong co neo thuc te.

Voi document/bao cao tieng Viet cua sinh vien, viet tu nhien, ro y, khong thoi phong, khong dung giong marketing. Neu can bao cao ket qua test, phai ghi ket qua that va noi ro neu chua chay test.
