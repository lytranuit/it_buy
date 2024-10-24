<template>
  <Dialog
    v-model:visible="visibleDialog"
    :header="headerForm"
    :modal="true"
    class="p-fluid"
    style="width: 70vw"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }"
  >
    <div class="row mb-2">
      <div class="field col">
        <label for="name"
          >Mã thương mại<span class="text-danger">*</span></label
        >
        <input-text
          class="p-inputtext-sm"
          v-model.trim="model.mahh"
          required="true"
          :class="{ 'p-invalid': submitted && !model.mahh }"
        />
        <small class="p-error" v-if="submitted && !model.mahh">Required.</small>
      </div>
      <div class="field col">
        <label for="name"
          >Tên thương mại<span class="text-danger">*</span></label
        >
        <input-text
          class="p-inputtext-sm"
          v-model.trim="model.tenhh"
          required="true"
          :class="{ 'p-invalid': submitted && !model.tenhh }"
        />
        <small class="p-error" v-if="submitted && !model.tenhh"
          >Required.</small
        >
      </div>
      <div class="field col">
        <label for="name">ĐVT</label>
        <InputText class="p-inputtext-sm" v-model.trim="model.dvt" />
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Mã gốc</label>
        <input-text class="p-inputtext-sm" v-model.trim="model.mahh_goc" />
      </div>
      <div class="field col">
        <label for="name">Tên gốc</label>
        <input-text class="p-inputtext-sm" v-model.trim="model.tenhh_goc" />
      </div>
      <div class="field col">
        <label for="name">Hạn dùng</label>
        <InputText
          class="p-inputtext-sm"
          type="number"
          v-model.trim="model.handung"
        />
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Tên hoạt chất</label>
        <textarea
          class="form-control"
          v-model.trim="model.tenhoatchat"
        ></textarea>
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Khu vực <span class="text-danger">*</span></label>
        <div>
          <KhuvucTreeSelect v-model="model.mapl"></KhuvucTreeSelect>
        </div>
      </div>
      <div class="field col">
        <label for="name">Dạng bào chế</label>
        <InputText class="p-inputtext-sm" v-model.trim="model.dangbaoche" />
      </div>
      <div class="field col">
        <label for="name">Qui cách</label>
        <InputText class="p-inputtext-sm" v-model.trim="model.quicachdonggoi" />
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Số dk</label>
        <InputText class="p-inputtext-sm" v-model.trim="model.sodk" />
      </div>
      <div class="field col">
        <label for="name">Ngày cấp số dk</label>
        <Calendar
          v-model="model.ngaycapsodk"
          dateFormat="yy-mm-dd"
          class="date-custom"
          :manualInput="false"
          showIcon
          showButtonBar
        />
      </div>
      <div class="field col">
        <label for="name">Ngày hết hạn số dk</label>
        <Calendar
          v-model="model.ngayhethansodk"
          dateFormat="yy-mm-dd"
          class="date-custom"
          :manualInput="false"
          showIcon
          showButtonBar
        />
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Ghi chú</label>
        <textarea class="form-control" v-model.trim="model.ghichu"></textarea>
      </div>
    </div>
    <template #footer>
      <Button
        label="Cancel"
        icon="pi pi-times"
        class="p-button-text"
        @click="hideDialog"
      ></Button>
      <Button
        label="Save"
        icon="pi pi-check"
        class="p-button-text"
        @click="save"
      ></Button>
    </template>
  </Dialog>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import productApi from "../../api/productApi";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import { useProduct } from "../../stores/product";
import KhuvucTreeSelect from "../TreeSelect/KhuvucTreeSelect.vue";

const toast = useToast();
const store_product = useProduct();
const { visibleDialog, headerForm, model } = storeToRefs(store_product);
const submitted = ref(false);
const hideDialog = () => {
  visibleDialog.value = false;
  submitted.value = false;
};

const emit = defineEmits(["save"]);
const save = () => {
  submitted.value = true;
  if (!valid()) return;
  // waiting.value = true;
  productApi.save(model.value).then((res) => {
    // waiting.value = false;
    visibleDialog.value = false;
    if (res.success) {
      if (model.value.old_key != null) {
        //edit
        toast.add({
          severity: "success",
          summary: "Thành công",
          detail: "Cập nhật " + model.value.mahh + " thành công",
          life: 3000,
        });
      } else {
        toast.add({
          severity: "success",
          summary: "Thành công",
          detail: "Tạo mới thành công",
          life: 3000,
        });
      }
    } else {
      toast.add({
        severity: "error",
        summary: "Lỗi",
        detail: res.message,
        life: 3000,
      });
    }
    emit("save", res);
    // loadLazyData();
  });
};

///Form
const valid = () => {
  if (!model.value.mahh.trim()) return false;
  if (!model.value.tenhh.trim()) return false;
  if (!model.value.mapl.trim()) return false;
  return true;
};
</script>