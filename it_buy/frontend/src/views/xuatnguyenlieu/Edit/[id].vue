<template>
  <div class="row clearfix">
    <div class="col-12">
      <div class="card mb-2">
        <div class="card-body">
          <div class="flex-m">
            <h5 class="title">
              <span> {{ model.name }}</span>
            </h5>
          </div>
          <div class="flex-m">
            <span class=""><span class="">ID</span>:
              <span class="font-weight-bold">{{ model.code }}</span></span><span class="mx-2">|</span>
            <div class="">
              <span class="">Người tạo</span>:
              <span class="font-weight-bold">{{
                user_created_by.FullName
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
          <TabView v-model:activeIndex="tabviewActive">
            <TabPanel>
              <template #header> Quy trình </template>
              <div class="row">
                <div class="col-12">
                  <Panel header="Dự trù hàng hóa" :toggleable="true">
                    <div class="row">
                      <div class="col-md-12">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Tiêu đề:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <input class="form-control" v-model="model.name" required
                              :disabled="model.status_id != 1" />
                          </div>
                        </div>
                      </div>
                      <div class="col-md-3">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Độ ưu tiên:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <select class="form-control" v-model="model.priority_id" :disabled="model.status_id != 1">
                              <option value="1">Bình thường</option>
                              <option value="2">Ưu tiên</option>
                              <option value="3">Gấp</option>
                            </select>
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
                              :manualInput="false" showIcon :minDate="minDate" :disabled="model.status_id != 1" />
                          </div>
                        </div>
                      </div>
                      <div class="col-md-3">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">
                            Tổng giá trị dự kiến
                          </b>
                          <div class="col-12 col-lg-12 pt-1">
                            <input class="form-control" v-model="model.tonggiatri" :disabled="model.status_id != 1" />
                          </div>
                        </div>
                      </div>
                      <div class="col-md-12">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Lý do mua hàng:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <textarea class="form-control form-control-sm" v-model="model.note" required
                              :disabled="model.status_id != 1"></textarea>
                          </div>
                        </div>
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Hàng hóa:<i class="text-danger">*</i></b>
                          <div class="col-12 col-lg-12 pt-1">
                            <FormDutruChitiet></FormDutruChitiet>
                          </div>
                          <div class="col-12 col-lg-12 pt-4">
                            <TableHanghoa :dutru_id="route.params.id"></TableHanghoa>
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
                  <Panel header="Đề nghị mua hàng" v-if="model.status_id == 4">
                    <div class="row">
                      <div class="col-md-12" v-if="list_muahang.length > 0">
                        <TabView>
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
                              {{ item.muahang.code }} -
                              {{ item.muahang.muahang_chonmua.ncc.tenncc }}
                            </template>
                            <div class="row m-0">
                              <div class="col-md-4 form-group">
                                <b class="">Ngày giao hàng dự kiến:</b>
                                <div class="mt-2">
                                  <Calendar :modelValue="formatDate(item.muahang.date)" dateFormat="yy-mm-dd"
                                    class="date-custom" :manualInput="false" showIcon :minDate="minDate"
                                    :disabled="item.muahang.is_dathang" />
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
                      <div class="col-md-12">
                        <b class="text-success">Hoàn thành</b>
                      </div>
                    </div>
                  </Panel>
                </div>
              </div>
            </TabPanel>
            <TabPanel>
              <template #header> Files </template>
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
import TabView from "primevue/tabview";
import TabPanel from "primevue/tabpanel";
import Stepper from "primevue/stepper";
import Divider from "primevue/divider";
import Panel from "primevue/panel";
import Steps from "primevue/steps";
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import Loading from "../../../components/Loading.vue";
import FormDutruChitiet from "../../../components/Datatable/FormDutruChitiet.vue";
import DepartmentTreeSelect from "../../../components/TreeSelect/DepartmentTreeSelect.vue";
import { useRoute, useRouter } from "vue-router";
import dutruApi from "../../../api/dutruApi";
import { useDutru } from "../../../stores/dutru";

import { storeToRefs } from "pinia";
import { useGeneral } from "../../../stores/general";
import moment from "moment";
import FormDutruNhanhang from "../../../components/Datatable/FormDutruNhanhang.vue";
import { download, formatDate } from "../../../utilities/util";
import Comment from "../../../components/dutru/Comment.vue";
import DutruFiles from "../../../components/Datatable/DutruFiles.vue";
import FormDutruChitiet2 from "../../../components/Datatable/FormDutruChitiet2.vue";
import TableHanghoa from "../../../components/hanghoa/TableHanghoa.vue";

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
const storeDutru = useDutru();
const store_general = useGeneral();
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
} = storeToRefs(storeDutru);

const muahangActive = (muahang) => {
  let ret = 0;
  if (muahang.is_thanhtoan) {
    ret = 5;
  } else if (muahang.is_dathang) {
    ret = 4;
  } else if ([8, 9, 10, 11].indexOf(muahang.status_id) != -1) {
    ret = 3;
  } else if ([7].indexOf(muahang.status_id) != -1) {
    ret = 2;
  } else if ([6].indexOf(muahang.status_id) != -1) {
    ret = 1;
  }
  return ret;
};
const load_data = async (id) => {
  waiting.value = true;

  var all = [
    dutruApi.get(id),
    dutruApi.getNhanhang(id),
    dutruApi.getMuahang(id),
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
  list_nhanhang.value = response[1];
  list_muahang.value = response[2];

  waiting.value = false;
};
onMounted(() => {
  store_general.fetchMaterialGroup();
  store_general.fetchMaterials();

  load_data(route.params.id);
});
const submit = async () => {
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
  if (!model.value.bophan_id) {
    alert("Chưa chọn bộ phận!");
    buttonDisabled.value = false;
    return false;
  }
  if (datatable.value.length) {
    for (let product of datatable.value) {
      if (!product.tenhh) {
        alert("Chưa nhập hàng hóa!");
        return false;
      }
      if (model.value.type_id == 1) {
        if (!product.grade) {
          alert("Chưa nhập Grade!");
          return false;
        }
        if (!product.tensp) {
          alert("Chưa nhập Tên SP!");
          return false;
        }
        if (!product.dangbaoche) {
          alert("Chưa nhập Dạng bào chế!");
          return false;
        }
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
  model.value.list_add = list_add.value;
  model.value.list_update = list_update.value;
  model.value.list_delete = list_delete.value;

  var params = model.value;
  $(".hinhanh-file-input").each(function (index) {
    // console.log(this)
    var files = $(this)[0].files;
    var key = $(this).data("key");
    for (var stt = 0; stt < files.length; stt++) {
      var file = files[stt];
      params["file_" + key + "_" + stt] = file;
    }
  });
  waiting.value = true;
  var response = await dutruApi.save(model.value);
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
};
const view = async () => {
  await submit();
  waiting.value = true;
  var response = await dutruApi.xuatpdf(model.value.id, true);
  waiting.value = false;
  if (response.success) {
    window.open(response.link, "_blank").focus();
  }
};
const xuatpdf = async () => {
  await submit();
  waiting.value = true;
  var response = await dutruApi.xuatpdf(model.value.id);

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
const savenhanhang = async () => {
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
  waiting.value = true;
  var response = await dutruApi.savenhanhang({
    dutru_id: model.value.id,
    list: items,
  });
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
};
</script>
<style>
.p-Panel .p-Panel -toggleable .p-Panel -header {
  background-color: transparent;
  border: 0;
}
</style>
