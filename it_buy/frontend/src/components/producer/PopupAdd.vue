<template>
  <Dialog
    v-model:visible="visibleDialog"
    :header="headerForm"
    :modal="true"
    class="p-fluid"
  >
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Mã<span class="text-danger">*</span></label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.mansx"
          required="true"
          :class="{ 'p-invalid': submitted && !model.mansx }"
        />
        <small class="p-error" v-if="submitted && !model.mansx"
          >Required.</small
        >
      </div>
      <div class="field col">
        <label for="name">Tên<span class="text-danger">*</span></label>
        <input-text
          id="name"
          class="p-inputtext-sm"
          v-model.trim="model.tennsx"
          required="true"
          :class="{ 'p-invalid': submitted && !model.tennsx }"
        />
        <small class="p-error" v-if="submitted && !model.tennsx"
          >Required.</small
        >
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
import producerApi from "../../api/producerApi";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import { useProducer } from "../../stores/producer";

const toast = useToast();
const store_producer = useProducer();
const { visibleDialog, headerForm, model } = storeToRefs(store_producer);
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
  producerApi.save(model.value).then((res) => {
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
  if (!model.value.mansx.trim()) return false;
  if (!model.value.tennsx.trim()) return false;
  return true;
};
</script>