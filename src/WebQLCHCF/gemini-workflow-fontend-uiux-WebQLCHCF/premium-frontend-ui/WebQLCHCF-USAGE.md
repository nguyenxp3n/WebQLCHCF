# Cách dùng premium-frontend-ui cho WebQLCHCF

Dùng sau khi đã có design system.

## Áp dụng vào Dashboard

Cải thiện:

```text
- Card doanh thu hôm nay.
- Card doanh thu tháng.
- Card nhân viên đang làm.
- Card bàn đang hoạt động.
- Card sản phẩm đang bán.
- Card nguyên liệu sắp hết.
```

Mẫu class nên dùng:

```html
<div class="card app-card border-0 shadow-sm rounded-4">
  <div class="card-body">
    <div class="d-flex justify-content-between align-items-center">
      <div>
        <p class="text-muted small mb-1">Doanh thu hôm nay</p>
        <h3 class="fw-bold mb-0">@Model.DoanhThuHomNay</h3>
      </div>
      <div class="app-icon bg-success-subtle text-success">
        <i class="bi bi-cash-stack"></i>
      </div>
    </div>
  </div>
</div>
```

## Áp dụng vào table

Dùng cho danh sách sản phẩm, hóa đơn, phiếu nhập:

```html
<div class="card app-card border-0 shadow-sm rounded-4">
  <div class="card-header bg-white border-0">
    <h5 class="mb-0 fw-semibold">Danh sách sản phẩm</h5>
  </div>
  <div class="table-responsive">
    <table class="table table-hover align-middle mb-0">
      ...
    </table>
  </div>
</div>
```

## Không được làm

Không tạo hiệu ứng bay, phóng to mạnh, custom cursor, scroll animation, background 3D.
