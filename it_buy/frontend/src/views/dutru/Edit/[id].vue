<template>
  <div class="row clearfix">
    <div class="col-12">
      <div class="card mb-2">
        <div class="card-body">
          <div class="flex-m">
            <h5 class="title">
              <span>
                {{ model.name }}</span>
            </h5>

          </div>
          <div class="flex-m"><span class=""><span class="">ID</span>: <span class="font-weight-bold">{{ model.code
                }}</span></span><span class="mx-2">|</span>
            <div class=""><span class="">Người tạo</span>: <span class="font-weight-bold">{{ user_created_by.fullName
                }}</span></div><span class="mx-2">|</span>
            <div class=""><span class=""> Ngày tạo: </span><span class="font-weight-bold">{{
                  formatDate(model.created_at) }}</span></div>
          </div>
        </div>
      </div>
    </div>
    <div class="col-12">
      <section class="card">
        <div class="card-body">
          <TabView v-model:activeIndex="tabviewActive">
            <TabPanel>
              <template #header>
                Quy trình
              </template>
              <div class="row">
                <div class="col-12">
                  <Panel header="Dự trù hàng hóa" :toggleable="true">
                    <div class="row">
                      <div class="col-md-6">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Tiêu đề:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <input class="form-control" v-model="model.name" required :readonly="model.status_id != 1">
                          </div>
                        </div>
                      </div>
                      <div class="col-md-3">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Bộ phận dự trù:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <DepartmentTreeSelect v-model="model.bophan_id" :clearable="false"
                              :disabled="model.status_id != 1">
                            </DepartmentTreeSelect>

                          </div>
                        </div>
                      </div>
                      <div class="col-md-3">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Hạn giao hàng:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <Calendar v-model="model.date" dateFormat="yy-mm-dd" class="date-custom"
                              :manualInput="false" showIcon :minDate="minDate" :readonly="model.status_id != 1" />
                          </div>
                        </div>
                      </div>
                      <div class="col-md-12">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Lý do mua hàng:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <textarea class="form-control form-control-sm" v-model="model.note" required
                              :readonly="model.status_id != 1"></textarea>
                          </div>
                        </div>
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Hàng hóa:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <FormDutruChitiet></FormDutruChitiet>
                          </div>
                        </div>
                      </div>
                      <div class="col-md-12 text-center" v-if="model.status_id == 1">
                        <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
                          @click.prevent="submit()"></Button>
                        <Button label="Xuất PDF và trình ký" icon="pi pi-file" class="p-button-sm mr-2"
                          @click.prevent="xuatpdf()"></Button>
                      </div>

                    </div>
                  </Panel>
                </div>
                <div class="col-12 mt-3">
                  <Panel header="Trình ký" :toggleable="true">
                    <div class="row">
                      <div class="col-md-12 text-center">
                        <a :href="model.pdf" :download="model.pdf"
                          class="download-icon-link d-inline-flex align-items-center">
                          <i class="far fa-file text-danger" style="font-size: 40px; margin-right: 10px;"></i>
                          {{ model.pdf }}
                        </a>
                        <!-- <div class="d-inline-block ml-5" v-if="model.status_id == 2">
                          <a :href="path_esign + '/admin/document/create?queue_id=' + model.esign_id">
                            <Button label="Bắt đầu trình ký" class="p-button-primary p-button-sm mr-2"
                              icon="fas fa-long-arrow-alt-right"></Button>
                          </a>
                        </div> -->

                        <div class="d-inline-block ml-5" v-if="model.status_id == 3">
                          <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                            <Button label="Đang trình ký" class="p-button-warning p-button-sm mr-2"
                              icon="fas fa-spinner fa-spin"></Button>
                          </a>
                          <a :href="path_esign + '/admin/document/edit/' + model.esign_id">
                            <Button label="Đi đến cài đặt" class="p-button-sm mr-2" icon="fas fa-cog"></Button>
                          </a>
                        </div>

                        <div class="d-inline-block ml-5" v-else-if="model.status_id == 4">
                          <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                            <Button label="Đã hoàn thành" class="p-button-success p-button-sm mr-2"
                              icon="fas fa-thumbs-up"></Button>
                          </a>
                        </div>

                        <div class="d-inline-block ml-5" v-else-if="model.status_id == 5">
                          <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                            <Button label="Không duyệt" class="p-button-danger p-button-sm mr-2"
                              icon="fas fa-times"></Button>
                          </a>
                        </div>
                      </div>
                    </div>
                  </Panel>
                </div>
                <div class="col-12 mt-3">
                  <Panel header="Đề nghị mua hàng" v-if="model.status_id == 4">
                    <div class="row">
                      <div class="col-md-12" v-if="list_muahang.length > 0">
                        <TabView v-model:activeIndex="active1">
                          <TabPanel v-for="(item, key) in list_muahang" :key="key">
                            <template #header>
                              {{ item.code }} - {{ item.name }}
                            </template>
                            <div class="row">
                              <div class="col-12">
                                <Steps :model="items" :activeStep="muahangActive(item)" />
                              </div>
                            </div>
                          </TabPanel>
                        </TabView>
                      </div>
                    </div>
                  </Panel>
                </div>
                <div class="col-12 mt-3">
                  <Panel header="Nhận hàng" v-if="model.status_id == 4">
                    <div class="row">
                      <div class="col-md-12" v-if="list_nhanhang.length > 0">
                        <TabView v-model:activeIndex="active">
                          <TabPanel v-for="(item, key) in list_nhanhang" :key="key">
                            <template #header>
                              {{ item.muahang.code }} - {{ item.muahang.muahang_chonmua.ncc.tenncc }}
                            </template>
                            <div class="row m-0">
                              <div class="col-md-4 form-group">
                                <b class="">Ngày giao hàng dự kiến:</b>
                                <div class="mt-2">
                                  <Calendar :modelValue="formatDate(item.muahang.date)" dateFormat="yy-mm-dd"
                                    class="date-custom" :manualInput="false" showIcon :minDate="minDate"
                                    :readonly="item.muahang.is_dathang" />
                                </div>
                              </div>
                              <div class="col-md-12 mb-2">
                                <FormDutruNhanhang :index="key"></FormDutruNhanhang>
                              </div>
                              <div class="col-md-12 text-center mt-3" v-if="!model.date_finish">
                                <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
                                  @click.prevent="savenhanhang()"></Button>
                              </div>
                            </div>
                          </TabPanel>
                        </TabView>
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
                      <div class="col-md-12 ">
                        <b class="text-success">Hoàn thành</b>
                      </div>
                    </div>
                  </Panel>
                </div>
              </div>
            </TabPanel>
            <TabPanel>
              <template #header>
                Files
              </template>
              <DutruFiles></DutruFiles>
            </TabPanel>
          </TabView>
        </div>
      </section>
    </div>
    <div class="col-md-12 mt-2" v-if="model.id > 0">
      <Comment></Comment>
    </div>
    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>

import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../../stores/auth";
import { useToast } from "primevue/usetoast";
import TabView from 'primevue/tabview';
import TabPanel from 'primevue/tabpanel';
import Stepper from 'primevue/stepper';
import Divider from 'primevue/divider';
import Panel from 'primevue/panel';
import Steps from 'primevue/steps';
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import Loading from "../../../components/Loading.vue";
import FormDutruChitiet from '../../../components/Datatable/FormDutruChitiet.vue'
import DepartmentTreeSelect from "../../../components/TreeSelect/DepartmentTreeSelect.vue";
import { useRoute, useRouter } from "vue-router";
import dutruApi from "../../../api/dutruApi";
import { useDutru } from '../../../stores/dutru';

import { storeToRefs } from "pinia";
import { useGeneral } from "../../../stores/general";
import moment from "moment";
import FormDutruNhanhang from "../../../components/Datatable/FormDutruNhanhang.vue";
import { formatDate } from "../../../utilities/util";
import Comment from "../../../components/dutru/Comment.vue";
import DutruFiles from "../../../components/Datatable/DutruFiles.vue";

const items = ref([
  {
    label: "Đề nghị mua hàng",
  },
  {
    label: "Báo giá"
  },
  {
    label: "So sánh báo giá"
  },
  {
    label: "Trình ký"
  },
  {
    label: "Đặt hàng"
  },
  {
    label: "Thanh toán"
  },
])
const active = ref(0);
const activeStep = ref(0);
const path_esign = import.meta.env.VITE_ESIGNURL;
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storeDutru = useDutru();
const { model, datatable, list_add, list_update, list_delete, list_nhanhang, tabviewActive, waiting, list_muahang, user_created_by } = storeToRefs(storeDutru);

const muahangActive = (muahang) => {
  let ret = 0;
  if (muahang.is_thanhtoan) {
    ret = 5;
  } else if (muahang.is_dathang) {
    ret = 4;
  } else if ([8, 9, 10, 11].indexOf(muahang.status_id) != -1) {
    ret = 3
  } else if ([7].indexOf(muahang.status_id) != -1) {
    ret = 2
  } else if ([6].indexOf(muahang.status_id) != -1) {
    ret = 1
  }
  return ret;
}
onMounted(() => {
  dutruApi.get(route.params.id).then((res) => {
    var chitiet = res.chitiet;
    var user = res.user_created_by;
    res.date = moment(res.date).format("YYYY-MM-DD");
    delete res.chitiet;
    delete res.user_created_by;
    user_created_by.value = user;
    model.value = res;
    datatable.value = chitiet;
    if ([2, 3, 5].indexOf(model.value.status_id) != -1) {
      activeStep.value = 1;
    } else if ([4].indexOf(model.value.status_id) != -1) {
      activeStep.value = 2;
    }
    if (model.value.date_finish) {
      activeStep.value = 3;
    }
  });
  dutruApi.getNhanhang(route.params.id).then((res) => {
    list_nhanhang.value = res;
    // console.log(res);
  });
  dutruApi.getMuahang(route.params.id).then((res) => {
    list_muahang.value = res;
  });
});
const submit = () => {
  if (!model.value.name) {
    alert("Chưa nhập tiêu đề!");
    return false;
  }
  if (!model.value.note) {
    alert("Chưa nhập lý do mua hàng!");
    return false;
  }
  if (!model.value.date) {
    alert("Chưa chọn hạn giao hàng!");
    return false;
  }
  if (datatable.value.length) {
    for (let product of datatable.value) {
      if (!product.mahh) {
        alert("Chưa chọn Mã NVL!");
        return false;
      }

      if (!product.dvt) {
        alert("Chưa nhập đơn vị tính!");
        return false;
      }
      if (!(product.soluong > 0)) {
        alert("Chưa chọn nhập số lượng");
        return false;
      }
    }
  } else {
    alert("Chưa nhập nguyên liệu!");
    return false;
  }
  model.value.list_add = list_add.value
  model.value.list_update = list_update.value
  model.value.list_delete = list_delete.value
  waiting.value = true;
  dutruApi.save(model.value).then((response) => {
    waiting.value = false;
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
    }
    // console.log(response)
  });
};
const xuatpdf = () => {
  waiting.value = true;
  dutruApi.xuatpdf(model.value.id).then((response) => {
    waiting.value = false;
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Xuất file thành công', life: 3000 });
      location.reload();
    }

    // console.log(response)
  });
}
const savenhanhang = () => {
  // console.log(list_nhanhang);
  var items = [];
  for (var list of list_nhanhang.value) {
    for (var item of list.items) {
      ///VAILD
      if (item.status_nhanhang == 1 && !item.date_nhanhang) {
        alert("Chưa nhập ngày nhận hàng!");
        return false;
      }
      ///VAILD
      if (item.status_nhanhang == 2 && !item.note_nhanhang) {
        alert("Vui lòng nhập nội dung khiếu nại!");
        return false;
      }
      delete item.muahang;
      items.push(item);
    }
  }
  // console.log(items);
  // return false;
  dutruApi.savenhanhang({ dutru_id: model.value.id, list: items }).then((response) => {
    waiting.value = false;
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
      location.reload();
    }
  });
}
</script>
<style>
.p-Panel .p-Panel -toggleable .p-Panel -header {
  background-color: transparent;
  border: 0;
}
</style>
