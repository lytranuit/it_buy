<template>
  <div class="row clearfix">
    <div class="col-12">
      <div class="card mb-2">
        <div class="card-body">
          <div class="flex-m">
            <h5 class="title">
              <span> {{ model.code }}</span>
            </h5>
          </div>
          <div class="flex-m">
            <div class="">
              <span class="">Người tạo</span>:
              <span class="font-weight-bold">{{
                model.created_by
              }}</span>
            </div>
            <span class="mx-2">|</span>
            <div class="">
              <span class=""> Ngày tạo: </span><span class="font-weight-bold">{{
                formatDate(model.created_at)
              }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="col-12">
      <section class="card">
        <div class="card-body">

          <div class="row">
            <div class="col-12">
              <Panel header="Đề nghị" :toggleable="true">
                <div class="row">
                  <div class="col-md-3">
                    <div class="form-group row">
                      <b class="col-12 col-lg-12 col-form-label">Bộ phận đề nghị:<i class="text-danger">*</i></b>
                      <div class="col-12 col-lg-12 pt-1">
                        <KhoTreeSelect v-model="model.bophan_id" :multiple="false" :disabled="model.status_id != 1">
                        </KhoTreeSelect>
                      </div>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="form-group row">
                      <b class="col-12 col-lg-12 col-form-label">Ngày mong muốn xuất:<i class="text-danger">*</i></b>
                      <div class="col-12 col-lg-12 pt-1">
                        <Calendar v-model="model.date" dateFormat="yy-mm-dd" class="date-custom" :manualInput="false"
                          showIcon :minDate="minDate" :disabled="model.status_id != 1" />
                      </div>
                    </div>
                  </div>
                  <div class="col-md-12">
                    <div class="form-group row">
                      <b class="col-12 col-lg-12 col-form-label">Lý do:<i class="text-danger">*</i></b>
                      <div class="col-12 col-lg-12 pt-1">
                        <textarea class="form-control form-control-sm" v-model="model.note" required
                          :disabled="model.status_id != 1"></textarea>
                      </div>
                    </div>
                    <div class="form-group row">
                      <b class="col-12 col-lg-12 col-form-label">Nguyên vật liệu:<i class="text-danger">*</i></b>
                      <div class="col-12 col-lg-12 pt-1">
                        <FormxuatnguyenlieuChitiet></FormxuatnguyenlieuChitiet>
                      </div>
                    </div>
                  </div>

                </div>
                <div class="col-md-12 text-center" v-if="model.status_id == 1">
                  <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
                    @click.prevent="submit()"></Button>
                  <Button label="Xem trước" icon="pi pi-eye" class="p-button-info p-button-sm mr-2"
                    @click.prevent="view()"></Button>
                  <Button label="Xuất PDF và trình ký" icon="pi pi-file" class="p-button-sm mr-2"
                    @click.once="xuatpdf()" :disabled="waiting"></Button>
                </div>
              </Panel>
            </div>
            <div class="col-12 mt-3">
              <Panel header="Trình ký" :toggleable="true">
                <div class="row">
                  <div class="col-md-12 text-center">
                    <a :href="model.pdf" target="_blank" :download="download(model.pdf)"
                      class="download-icon-link d-inline-flex align-items-center">
                      <i class="far fa-file text-danger" style="font-size: 40px; margin-right: 10px"></i>
                      {{ model.pdf }}
                    </a>
                    <!-- <div class="d-inline-block ml-5" v-if="model.status_id == 2">
                          <a :href="path_esign + '/admin/document/create?queue_id=' + model.esign_id">
                            <Button label="Bắt đầu trình ký" class="p-button-primary p-button-sm mr-2"
                              icon="fas fa-long-arrow-alt-right"></Button>
                          </a>
                        </div> -->

                    <div class="d-inline-block ml-5" v-if="model.status_id == 3">
                      <a :href="path_esign +
                        '/admin/document/details/' +
                        model.esign_id
                        ">
                        <Button label="Đang trình ký" class="p-button-warning p-button-sm mr-2"
                          icon="fas fa-spinner fa-spin"></Button>
                      </a>
                      <a :href="path_esign +
                        '/admin/document/edit/' +
                        model.esign_id
                        ">
                        <Button label="Đi đến cài đặt" class="p-button-sm mr-2" icon="fas fa-cog"></Button>
                      </a>
                    </div>

                    <div class="d-inline-block ml-5" v-else-if="model.status_id == 4">
                      <a :href="path_esign +
                        '/admin/document/details/' +
                        model.esign_id
                        ">
                        <Button label="Đã hoàn thành" class="p-button-success p-button-sm mr-2"
                          icon="fas fa-thumbs-up"></Button>
                      </a>
                    </div>

                    <div class="d-inline-block ml-5" v-else-if="model.status_id == 5">
                      <a :href="path_esign +
                        '/admin/document/details/' +
                        model.esign_id
                        ">
                        <Button label="Không duyệt" class="p-button-danger p-button-sm mr-2"
                          icon="fas fa-times"></Button>
                      </a>
                    </div>
                  </div>
                </div>
              </Panel>
            </div>
            <div class="col-12 mt-3">
              <Panel header="Hoàn thành" v-if="model.date_finish">
                <div class="row text-center">
                  <div class="col-md-12">
                    <img src="/src/assets/images/Purchase_Success.png" />
                  </div>
                  <div class="col-md-12">
                    <b class="text-success">Hoàn thành</b>
                  </div>
                </div>
              </Panel>
            </div>
          </div>

        </div>
      </section>
    </div>
    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../../stores/auth";
import { useToast } from "primevue/usetoast";
import TabView from "primevue/tabview";
import TabPanel from "primevue/tabpanel";
import Stepper from "primevue/stepper";
import Divider from "primevue/divider";
import Panel from "primevue/panel";
import Steps from "primevue/steps";
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import Loading from "../../../components/Loading.vue";
import FormxuatnguyenlieuChitiet from "../../../components/xuatnguyenlieu/FormxuatnguyenlieuChitiet.vue";
import { useRoute, useRouter } from "vue-router";
import xuatnguyenlieuApi from "../../../api/xuatnguyenlieuApi";
import { usexuatnguyenlieu } from "../../../stores/xuatnguyenlieu";

import { storeToRefs } from "pinia";
import { useGeneral } from "../../../stores/general";
import moment from "moment";
import { download, formatDate } from "../../../utilities/util";
import KhoTreeSelect from "../../../components/TreeSelect/KhoTreeSelect.vue";

const store_auth = useAuth();
const { is_Cungung } = storeToRefs(store_auth);
const items = ref([
  {
    label: "Đề nghị mua hàng",
  },
  {
    label: "Báo giá",
  },
  {
    label: "So sánh báo giá",
  },
  {
    label: "Trình ký",
  },
  {
    label: "Đặt hàng",
  },
  {
    label: "Thanh toán",
  },
]);
const active = ref(0);
const activeStep = ref(0);
const path_esign = import.meta.env.VITE_ESIGNURL;
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storexuatnguyenlieu = usexuatnguyenlieu();
const store_general = useGeneral();
const buttonDisabled = ref(false);
const {
  model,
  datatable,
  list_add,
  list_update,
  list_delete,
  list_nhanhang,
  tabviewActive,
  waiting,
  list_muahang,
  user_created_by,
} = storeToRefs(storexuatnguyenlieu);

const load_data = async (id) => {
  waiting.value = true;

  var all = [
    xuatnguyenlieuApi.get(id),
  ];
  var response = await Promise.all(all);

  var chitiet = response[0].chitiet;
  var user = response[0].user_created_by;
  response[0].date = moment(response[0].date).format("YYYY-MM-DD");
  delete response[0].chitiet;
  delete response[0].user_created_by;
  user_created_by.value = user;
  model.value = response[0];
  datatable.value = chitiet;
  if ([2, 3, 5].indexOf(model.value.status_id) != -1) {
    activeStep.value = 1;
  } else if ([4].indexOf(model.value.status_id) != -1) {
    activeStep.value = 2;
  }
  if (model.value.date_finish) {
    activeStep.value = 3;
  }

  waiting.value = false;
};
onMounted(() => {
  store_general.fetchTonkhoNVL();
  load_data(route.params.id);
});
const submit = async () => {
  buttonDisabled.value = true;
  if (!model.value.note) {
    alert("Chưa nhập lý do mua hàng!");
    buttonDisabled.value = false;
    return false;
  }
  if (!model.value.date) {
    alert("Chưa chọn ngày mong muốn xuất!");
    buttonDisabled.value = false;
    return false;
  }
  if (!model.value.bophan_id) {
    alert("Chưa chọn bộ phận đề nghị!");
    buttonDisabled.value = false;
    return false;
  }
  if (datatable.value.length) {
    for (let product of datatable.value) {

      if (!product.mahh) {
        alert("Chưa có mã hàng hóa!");
        buttonDisabled.value = false;
        return false;
      }

      if (!product.tenhh) {
        alert("Chưa nhập hàng hóa!");
        buttonDisabled.value = false;
        return false;
      }
      if (!(product.soluong > 0)) {
        alert("Chưa chọn nhập số lượng");
        buttonDisabled.value = false;
        return false;
      }

      if (product.soluong > product.tonkho) {
        alert(product.mahh + " không đủ số lượng xuất!");
        buttonDisabled.value = false;
        return false;
      }
    }
  } else {
    alert("Chưa nhập nguyên liệu!");
    buttonDisabled.value = false;
    return false;
  }
  model.value.list_add = list_add.value;
  model.value.list_update = list_update.value;
  model.value.list_delete = list_delete.value;

  var params = model.value;

  waiting.value = true;
  var response = await xuatnguyenlieuApi.save(params);
  waiting.value = false;
  if (response.success) {
    toast.add({
      severity: "success",
      summary: "Thành công!",
      detail: "Thay đổi thành công",
      life: 3000,
    });
    load_data(model.value.id);
  }
  return true;
};
const view = async () => {
  await submit();
  waiting.value = true;
  var response = await xuatnguyenlieuApi.xuatpdf(model.value.id, true);
  waiting.value = false;
  if (response.success) {
    window.open(response.link, "_blank").focus();
  }
};
const xuatpdf = async () => {
  await submit();
  waiting.value = true;
  var response = await xuatnguyenlieuApi.xuatpdf(model.value.id);

  if (response.success) {
    toast.add({
      severity: "success",
      summary: "Thành công!",
      detail: "Xuất file thành công",
      life: 3000,
    });
    load_data(model.value.id);
  }
  waiting.value = false;
};

</script>
<style>
.p-Panel .p-Panel -toggleable .p-Panel -header {
  background-color: transparent;
  border: 0;
}
</style>
