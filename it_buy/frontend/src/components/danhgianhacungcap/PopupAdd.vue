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
          >Tên nguyên liệu <span class="text-danger">*</span></label
        >
        <div>
          <InputText
            v-model="model.tenhh"
            class="form-control form-control-sm"
          />
        </div>
      </div>
      <div class="field col">
        <label for="name">ĐVT <span class="text-danger">*</span></label>
        <div>
          <InputText v-model="model.dvt" class="form-control form-control-sm" />
        </div>
      </div>
      <div class="field col">
        <label for="name">Grade <span class="text-danger">*</span></label>
        <div>
          <InputText
            v-model="model.grade"
            class="form-control form-control-sm"
          />
        </div>
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name"
          >Nhà sản xuất <span class="text-danger">*</span></label
        >
        <div>
          <InputText
            v-model="model.nhasx"
            class="form-control form-control-sm"
          />
        </div>
      </div>
      <div class="field col">
        <label for="name"
          >Nhà phân phối <span class="text-danger">*</span></label
        >
        <div>
          <InputText
            v-model="model.nhacc"
            class="form-control form-control-sm"
          />
        </div>
      </div>
    </div>
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Mã số thiết kế</label>
        <div>
          <InputText
            v-model="model.masothietke"
            class="form-control form-control-sm"
          />
        </div>
      </div>
      <div class="field col">
        <label for="name">Qui cách</label>
        <div>
          <InputText
            v-model="model.quicach"
            class="form-control form-control-sm"
          />
        </div>
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
import danhgianhacungcapApi from "../../api/danhgianhacungcapApi";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import { useDanhgianhacungcap } from "../../stores/danhgianhacungcap";
import NsxTreeSelect from "../TreeSelect/NsxTreeSelect.vue";
import NccTreeSelect from "../TreeSelect/NccTreeSelect.vue";

const toast = useToast();
const store_danhgianhacungcap = useDanhgianhacungcap();
const { visibleDialog, headerForm, model } = storeToRefs(
  store_danhgianhacungcap
);
const submitted = ref(false);
const hideDialog = () => {
  visibleDialog.value = false;
  submitted.value = false;
};

const emit = defineEmits(["save"]);
const save = () => {
  if (!valid()) {
    alert("Nhập đầy dủ thông tin!");
    return false;
  }
  // waiting.value = true;
  danhgianhacungcapApi.save(model.value).then((res) => {
    // waiting.value = false;
    visibleDialog.value = false;
    if (res.success) {
      toast.add({
        severity: "success",
        summary: "Thành công",
        detail: "Thành công",
        life: 3000,
      });
      emit("save", res.data);
    } else {
      toast.add({
        severity: "error",
        summary: "Lỗi",
        detail: res.message,
        life: 3000,
      });
    }
    // loadLazyData();
  });
};

///Form
const valid = () => {
  if (!model.value.tenhh) return false;
  if (!model.value.dvt) return false;
  if (!model.value.nhacc) return false;
  if (!model.value.nhasx) return false;
  return true;
};
</script>