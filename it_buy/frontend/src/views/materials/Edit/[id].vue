<template>
  <div class="a">
    <div class="card card-fluid">
      <div class="card-header">
        <h5 class="mb-0">{{ model.mahh }} - {{ model.tenhh }}</h5>
      </div>
      <div class="card-body">
        <TabView>
          <TabPanel>
            <template #header>
              Thông tin chung
            </template>
            <div class="row mb-2">
              <div class="field col">
                <label for="name">Mã hàng hóa <span class="text-danger">*</span></label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.mahh" required="true"
                    :class="{ 'p-invalid': submitted && !model.mahh }" />
                  <small class="p-error" v-if="submitted && !model.mahh">Required.</small>
                </div>
              </div>
              <div class="field col">
                <label for="name">Tên hàng hóa <span class="text-danger">*</span></label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.tenhh" required="true"
                    :class="{ 'p-invalid': submitted && !model.tenhh }" />
                  <small class="p-error" v-if="submitted && !model.tenhh">Required.</small>
                </div>
              </div>
              <div class="field col">
                <label for="name">ĐVT<span class="text-danger">*</span></label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.dvt" required="true"
                    :class="{ 'p-invalid': submitted && !model.dvt }" />
                  <small class="p-error" v-if="submitted && !model.dvt">Required.</small>
                </div>
              </div>
            </div>
            <div class="row mb-2">
              <div class="field col">
                <label for="name">Mã Artwork</label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.masothietke" />
                </div>
              </div>
              <div class="field col">
                <label for="name">Grade</label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.grade" />
                </div>
              </div>
              <div class="field col">
                <label for="name">Nhà sản xuất</label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.nhasx" />
                </div>
              </div>
            </div>
            <div class="row mb-2">
              <div class="field col">
                <label for="name">Tiêu chuẩn</label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.tieuchuan" />
                </div>
              </div>
              <div class="field col">
                <label for="name">Tên sản phẩm</label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.tensp" />
                </div>
              </div>
              <div class="field col">
                <label for="name">Dạng bào chế</label>
                <div>
                  <InputText id="name" class="form-control form-control-sm" v-model.trim="model.dangbaoche" />
                </div>
              </div>

            </div>
            <div class="row mb-2">
              <div class="field col-12">
                <label for="name">Lưu ý đặc biệt</label>
                <textarea id="name" class="form-control form-control sm" v-model.trim="model.note"></textarea>
              </div>
            </div>
          </TabPanel>
          <TabPanel>
            <template #header>
              Nhà cung cấp
            </template>
          </TabPanel>
        </TabView>

      </div>
    </div>
    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>

import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useToast } from "primevue/usetoast";
import InputText from 'primevue/inputtext';
import TabView from 'primevue/tabview';
import TabPanel from 'primevue/tabpanel';
import Panel from 'primevue/panel';
import Loading from "../../../components/Loading.vue";
import { useRoute, useRouter } from "vue-router";
import materialApi from "../../../api/materialApi";
import { useMaterials } from '../../../stores/materials';

import { storeToRefs } from "pinia";
import moment from "moment";
const waiting = ref();
const store_materials = useMaterials();
const { model, submitted } = storeToRefs(store_materials);
const toast = useToast();
const minDate = new Date();
const route = useRoute();

const load_data = async (id) => {
  var res = await materialApi.get(route.params.id);
  model.value = res;
}
onMounted(() => {
  load_data(route.params.id);
});
</script>
