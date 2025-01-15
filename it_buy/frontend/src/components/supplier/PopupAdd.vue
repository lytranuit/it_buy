<template>
  <Dialog
    v-model:visible="visibleDialog"
    :header="headerForm"
    :modal="true"
    class="p-fluid"
    style="width: 75vw"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }"
  >
    <div class="row mb-2">
      <div class="field col">
        <label for="name"
          >Mã nhà cung cấp<span class="text-danger">*</span></label
        >
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.mancc"
          required="true"
          :class="{ 'p-invalid': submitted && !model.mancc }"
        />
        <small class="p-error" v-if="submitted && !model.mancc"
          >Required.</small
        >
      </div>
      <div class="field col">
        <label for="name"
          >Tên nhà cung cấp <span class="text-danger">*</span></label
        >
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.tenncc"
          required="true"
          :class="{ 'p-invalid': submitted && !model.tenncc }"
        />
        <small class="p-error" v-if="submitted && !model.tenncc"
          >Required.</small
        >
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Địa chỉ <span class="text-danger">*</span></label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.diachincc"
          :class="{ 'p-invalid': submitted && !model.diachincc }"
        />
        <small class="p-error" v-if="submitted && !model.diachincc"
          >Required.</small
        >
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Điện thoại<span class="text-danger">*</span></label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.dienthoaincc"
          :class="{ 'p-invalid': submitted && !model.dienthoaincc }"
        />
        <small class="p-error" v-if="submitted && !model.dienthoaincc"
          >Required.</small
        >
      </div>
      <div class="field col">
        <label for="name">Email</label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.emailncc"
        />
      </div>
      <div class="field col">
        <label for="name">Tài khoản ngân hàng</label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.taikhoannh"
        />
      </div>
      <div class="field col">
        <label for="name">Mã số thuế</label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.masothue"
        />
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Loại</label>
        <div>
          <select class="form-control form-control-sm" v-model="model.type">
            <option value="1">Mua hàng trực tiếp</option>
            <option value="2">Mua hàng gián tiếp</option>
          </select>
        </div>
      </div>
      <div class="field col">
        <label for="name">Đánh giá</label>
        <div>
          <Rating v-model="model.danhgia" :cancel="false" />
        </div>
      </div>
      <div class="field col"></div>
      <div class="field col"></div>
    </div>
    <div class="row mb-2">
      <div class="field col-12">
        <b>Người liên hệ</b>
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Họ Tên <span class="text-danger">*</span></label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.nguoilienhe"
          :class="{ 'p-invalid': submitted && !model.nguoilienhe }"
        />
        <small class="p-error" v-if="submitted && !model.nguoilienhe"
          >Required.</small
        >
      </div>
      <div class="field col">
        <label for="name">Chức vụ</label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.chucvu"
        />
      </div>
      <div class="field col">
        <label for="name">Email</label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.email"
        />
      </div>
      <div class="field col">
        <label for="name">Điện thoại</label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.dienthoai"
        />
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col-12">
        <b>Asta</b>
      </div>
    </div>

    <div class="row mb-2">
      <div class="field col">
        <label for="name"
          >Người phụ trách mua hàng <span class="text-danger">*</span></label
        >
        <div>
          <UserTreeSelect v-model="model.nguoiphutrach"></UserTreeSelect>
        </div>
      </div>
      <div class="field col"></div>
      <div class="field col"></div>
      <div class="field col"></div>
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
import supplierApi from "../../api/supplierApi";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import Rating from "primevue/rating";
import { useSupplier } from "../../stores/supplier";
import UserTreeSelect from "../TreeSelect/UserTreeSelect.vue";

const toast = useToast();
const store_supplier = useSupplier();
const { visibleDialog, headerForm, model } = storeToRefs(store_supplier);
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
  supplierApi.save(model.value).then((res) => {
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
  if (!model.value.mancc) {
    alert("Nhập mã nhà cung cấp");
    return false;
  }
  if (!model.value.tenncc) {
    alert("Nhập tên nhà cung cấp");
    return false;
  }

  if (!model.value.diachincc) {
    alert("Nhập địa chỉ nhà cung cấp");
    return false;
  }
  if (!model.value.dienthoaincc) {
    alert("Nhập điện thoại nhà cung cấp");
    return false;
  }

  if (!model.value.nguoilienhe) {
    alert("Nhập họ tên người liên hệ!");
    return false;
  }
  if (!model.value.nguoiphutrach) {
    alert("Chọn người phụ trách mua hàng!");
    return false;
  }
  return true;
};
</script>