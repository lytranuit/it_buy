<template>
  <Dialog v-model:visible="visibleDialog" :header="headerForm" :modal="true" class="p-fluid" style="width: 75vw"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }">
    <TabView>
      <TabPanel header="Thông tin chung">
        <div class="row mb-2">
          <div class="col-md-2">
            <div class="form-group">
              <b>Hình đại diện:<span class="text-danger">*</span></b>
              <div class="pt-1">
                <ImageManager @choose="choose" :image="VITE_BASEURL + model.image_url + '?token=' + user.key_private">
                </ImageManager>
              </div>
            </div>
          </div>
          <div class="col-md-10">
            <div class="row">
              <div class="field col-md">
                <label for="name">Mã nhóm<span class="text-danger">*</span></label>
                <Nhom v-model="model.nhom" :multiple="false" :disabled="model.nhom != 'Khac'"></Nhom>
                <small class="p-error" v-if="submitted && !model.nhom">Required.</small>
              </div>
              <div class="field col">
                <label for="name">Mã hàng hóa <span class="text-danger">*</span></label>
                <input-text id="name" class="p-inputtext-sm" v-model.trim="model.mahh" required="true"
                  :class="{ 'p-invalid': submitted && !model.mahh }" :disabled="model.nhom != 'Khac'" />
                <small class="p-error" v-if="submitted && !model.mahh">Required.</small>
              </div>
              <div class="field col">
                <label for="name">Tên hàng hóa <span class="text-danger">*</span></label>
                <input-text id="name" class="p-inputtext-sm" v-model.trim="model.tenhh" required="true"
                  :class="{ 'p-invalid': submitted && !model.tenhh }" :disabled="model.nhom != 'Khac'" />
                <small class="p-error" v-if="submitted && !model.tenhh">Required.</small>
              </div>
              <div class="field col">
                <label for="name">ĐVT<span class="text-danger">*</span></label>
                <input-text id="name" class="p-inputtext-sm" v-model.trim="model.dvt" required="true"
                  :class="{ 'p-invalid': submitted && !model.dvt }" :disabled="model.nhom != 'Khac'" />
                <small class="p-error" v-if="submitted && !model.dvt">Required.</small>
              </div>
            </div>

            <div class="row mb-2">
              <div class="field col">
                <label for="name">Nhà sản xuất</label>
                <NsxTreeSelect v-model="model.mansx" :required="true" :useID="false" :disabled="model.nhom != 'Khac'">
                </NsxTreeSelect>
              </div>
              <div class="field col">
                <label for="name">Nhà cung cấp</label>
                <NccTreeSelect v-model="model.mancc" :required="true" :useID="false" :disabled="model.nhom != 'Khac'">
                </NccTreeSelect>
              </div>
            </div>
          </div>
        </div>
        <div class="row mb-2">
          <div class="field col">
            <label for="name">Mã Artwork</label>
            <input-text id="name" class="p-inputtext-sm" v-model.trim="model.masothietke"
              :disabled="model.nhom != 'Khac'" />
          </div>
          <div class="field col">
            <label for="name">Grade</label>
            <input-text id="name" class="p-inputtext-sm" v-model.trim="model.grade" :disabled="model.nhom != 'Khac'" />
          </div>
        </div>
        <div class="row mb-2">
          <div class="field col-md-6">
            <label for="name">Leadtime</label>
            <Textarea id="leadtime" class="w-100" v-model.trim="model.leadtime" :autoResize="true"></Textarea>
          </div>
          <div class="field col-md-6">
            <label for="name">MOQ(Số lượng đặt hàng tối thiểu)</label>
            <Textarea id="moq" class="w-100" v-model.trim="model.moq" :autoResize="true"></Textarea>
          </div>
        </div>
        <div class="row mb-2">
          <div class="field col">
            <label for="name">Tiêu chuẩn</label>
            <input-text id="name" class="p-inputtext-sm" v-model.trim="model.tieuchuan" />
          </div>
        </div>
        <div class="row mb-2">
          <div class="field col-12">
            <label for="name">Lưu ý đặc biệt</label>
            <textarea id="name" class="form-control form-control sm" v-model.trim="model.note"></textarea>
          </div>
        </div>
      </TabPanel>
      <!-- <TabPanel header="Files">
        <MaterialsFiles></MaterialsFiles>
      </TabPanel> -->
      <TabPanel header="Hàng hóa cùng loại">
        <MaterialTreeSelect :multiple="true" v-model="model.list_sp"></MaterialTreeSelect>
        <!-- <MaterialsFiles></MaterialsFiles> -->
      </TabPanel>
      <TabPanel header="Lịch sử mua hàng" v-if="model.id > 0">
        <Lichsumuahang :mahh="model.mahh"></Lichsumuahang>
      </TabPanel>
      <TabPanel header="Cài đặt thông báo">
        <div class="row mb-2">
          <div class="field col-md-3">
            <label for="name">Khi tồn kho dưới</label>
            <input-number id="name" class="p-inputtext-sm" v-model.trim="model.tonkho_duoi"
              :suffix="' ' + model.dvt"></input-number>
          </div>
          <div class="field col-md-9">
            <label for="name">Thông báo cho</label>
            <UserTreeSelect v-model="model.list_user_notify" :multiple="true"></UserTreeSelect>
          </div>
        </div>
      </TabPanel>
    </TabView>

    <template #footer>
      <Button label="Cancel" icon="pi pi-times" class="p-button-text" @click="hideDialog"></Button>
      <Button label="Save" icon="pi pi-check" class="p-button-text" @click="saveProduct"></Button>
    </template>
  </Dialog>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import materialApi from "../../api/materialApi";
import Dialog from "primevue/dialog";
import InputNumber from "primevue/inputnumber";
import Button from "primevue/button";
import Textarea from "primevue/textarea";
import TabView from "primevue/tabview";
import TabPanel from "primevue/tabpanel";
import InputText from "primevue/inputtext";
import { useMaterials } from "../../stores/materials";
import NccTreeSelect from "../TreeSelect/NccTreeSelect.vue";
import NsxTreeSelect from "../TreeSelect/NsxTreeSelect.vue";
import MaterialsFiles from "../Datatable/MaterialsFiles.vue";
import MaterialTreeSelect from "../TreeSelect/MaterialTreeSelect.vue";
import Lichsumuahang from "./Lichsumuahang.vue";
import Nhom from "../TreeSelect/Nhom.vue";
import UserTreeSelect from "../TreeSelect/UserTreeSelect.vue";
import ImageManager from "../ImageManager.vue";
import { useAuth } from "../../stores/auth";

const VITE_BASEURL = import.meta.env.VITE_BASEURL;
// const props = defineProps({
//   type: {
//     type: String,
//     default: "NVL",
//   },
// });
const toast = useToast();
const store_materials = useMaterials();
const store = useAuth();
const { user } = storeToRefs(store);
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
  //   model.value.surfix = props.type;
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
  if (!model.value.nhom) return false;
  if (!model.value.mahh) return false;
  if (!model.value.tenhh) return false;
  if (!model.value.dvt) return false;
  return true;
};
const choose = (path) => {
  model.value.image_url = "/private/upload" + path;
};
onMounted(() => {
  console.log("mounted");
  if (model.value.id > 0) {
    materialApi.get(model.value.id).then((res) => {
      model.value = res;
    });
  }
});
</script>