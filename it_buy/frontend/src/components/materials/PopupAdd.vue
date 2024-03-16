<template>

    <Dialog v-model:visible="visibleDialog" :header="headerForm" :modal="true" class="p-fluid">
        <div class="row mb-2">
            <div class="field col">
                <label for="name">Mã hàng hóa <span class="text-danger">*</span></label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.mahh" required="true"
                    :class="{ 'p-invalid': submitted && !model.mahh }" />
                <small class="p-error" v-if="submitted && !model.mahh">Required.</small>
            </div>
            <div class="field col">
                <label for="name">Tên hàng hóa <span class="text-danger">*</span></label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.tenhh" required="true"
                    :class="{ 'p-invalid': submitted && !model.tenhh }" />
                <small class="p-error" v-if="submitted && !model.tenhh">Required.</small>
            </div>
            <div class="field col">
                <label for="name">ĐVT</label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.dvt" />
            </div>
        </div>
        <div class="row mb-2">
            <div class="field col">
                <label for="name">Mã Artwork</label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.masothietke" />
            </div>
            <div class="field col">
                <label for="name">Grade</label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.grade" />
            </div>
            <div class="field col">
                <label for="name">Nhà sản xuất</label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.nhasx" />
            </div>
        </div>
        <div class="row mb-2">
            <div class="field col">
                <label for="name">Tiêu chuẩn</label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.tieuchuan" />
            </div>
            <div class="field col">
                <label for="name">Tên sản phẩm</label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.tensp" />
            </div>
            <div class="field col">
                <label for="name">Dạng bào chế</label>
                <InputText id="name" class="p-inputtext-sm" v-model.trim="model.dangbaoche" />
            </div>

        </div>
        <div class="row mb-2">
            <div class="field col-12">
                <label for="name">Lưu ý đặc biệt</label>
                <textarea id="name" class="form-control form-control sm" v-model.trim="model.note"></textarea>
            </div>
        </div>
        <template #footer>
            <Button label="Cancel" icon="pi pi-times" class="p-button-text" @click="hideDialog"></Button>
            <Button label="Save" icon="pi pi-check" class="p-button-text" @click="saveProduct"></Button>
        </template>
    </Dialog>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from 'pinia';
import materialApi from '../../api/materialApi';
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import { useMaterials } from '../../stores/materials';

const toast = useToast();
const store_materials = useMaterials();
const { visibleDialog, headerForm, model } = storeToRefs(store_materials);
const submitted = ref(false);
const hideDialog = () => {
    visibleDialog.value = false;
    submitted.value = false;
};

const emit = defineEmits(["save"]);
const saveProduct = () => {
    submitted.value = true;
    if (!valid()) return;
    // waiting.value = true;
    materialApi.save(model.value).then((res) => {
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
    if (!model.value.mahh.trim()) return false;
    if (!model.value.tenhh.trim()) return false;
    return true;
};
</script>