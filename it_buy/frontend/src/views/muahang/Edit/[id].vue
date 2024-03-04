<template>
  <div class="row clearfix">
    <div class="col-12">
      <section class="card card-fluid">
        <AlertError :message="messageError" v-if="messageError" />
        <AlertSuccess :message="messageSuccess" v-if="messageSuccess" />

        <Stepper orientation="vertical" :activeStep="activeStep">
          <StepperPanel header="Đề nghị mua hàng">
            <template #content="{ nextCallback }">

              <form method="POST" id="form">
                <div class="card-body">
                  <div class="row">
                    <div class="col-md-9">
                      <div class="form-group row">
                        <b class="col-12 col-lg-12 col-form-label">Tiêu đề:<i class="text-danger">*</i></b>
                        <div class="col-12 col-lg-12 pt-1">
                          <input class="form-control" v-model="model.name" required :readonly="readonly">
                        </div>
                      </div>
                    </div>
                    <div class="col-md-12">
                      <div class="form-group row">
                        <b class="col-12 col-lg-12 col-form-label">Lý do mua hàng:<i class="text-danger">*</i></b>
                        <div class="col-12 col-lg-12 pt-1">
                          <textarea class="form-control" v-model="model.note" required :readonly="readonly"></textarea>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-12">
                      <div class="form-group row">
                        <b class="col-12 col-lg-12 col-form-label">Nguyên vật liệu:<i class="text-danger">*</i></b>
                        <div class="col-12 col-lg-12 pt-1">
                          <FormMuahangChitiet></FormMuahangChitiet>
                        </div>
                      </div>
                      <!-- <div class="form-group row">
                        <b class="col-12 col-lg-12 col-form-label">Ghi chú:</b>
                        <div class="col-12 col-lg-12 pt-1">
                          <textarea class="form-control form-control-sm" v-model="model.note"
                            :readonly="model.status_id != 1"></textarea>
                        </div>
                      </div> -->
                    </div>
                    <div class="col-md-12 text-center" v-if="model.status_id == 1">
                      <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
                        @click.prevent="submit()"></Button>
                      <Button label="Gửi và nhận báo giá" icon="far fa-paper-plane" class="p-button-sm mr-2"
                        @click.prevent="baogia()"></Button>
                    </div>

                  </div>
                </div>

              </form>
            </template>
          </StepperPanel>
          <StepperPanel header="Báo giá">
            <template #content="{ prevCallback, nextCallback }">
              <div class="col-12 text-center" v-if="readonly == false">
                <div class="row">
                  <div class="col-4">
                    <NccTreeSelect v-model="nccmodel"></NccTreeSelect>
                  </div>
                  <div class="col-2">
                    <Button label="Thêm nhà cung cấp" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"
                      @click.prevent="addNCC()"></Button>
                  </div>
                </div>

              </div>
              <div class="col-12" v-if="nccs.length > 0">
                <TabView v-model:activeIndex="active">
                  <TabPanel v-for="(item, key) in nccs" :key="key">
                    <template #header>
                      {{ item.ncc.tenncc }}
                    </template>
                    <div class="row m-0">
                      <div class="col-12 text-right" v-if="readonly == false">
                        <Button label="Loại bỏ" icon="pi pi-times" class="p-button-danger" size="small"
                          @click.prevent="removeNCC(item)"></Button>
                      </div>

                      <div class="col-md-6">
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Đáp ứng các yêu cầu về hàng hóa:</b>
                          <div class="col-12 col-lg-12 pt-1">
                            <div class="custom-control custom-switch switch-success" style="height: 36px;;">
                              <input type="checkbox" :id="'dapung_' + key" class="custom-control-input"
                                v-model="item.dapung">
                              <label :for="'dapung_' + key" class="custom-control-label"></label>
                            </div>
                          </div>

                        </div>
                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Thời gian giao hàng:</b>
                          <div class="col-12 col-lg-12 pt-1">
                            <input class="form-control form-control-sm" v-model="item.thoigiangiaohang" />
                          </div>
                        </div>
                        <!-- ncc.thoigiangiaohang = "";
                        ncc.baohanh = "";
                        ncc.thanhtoan = "";
                        ncc.dapung = true;
                        ncc.tonggiatri = 0; -->
                      </div>
                      <div class="col-md-6">

                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Chính sách bảo hành:</b>
                          <div class="col-12 col-lg-12 pt-1">
                            <input class="form-control form-control-sm" v-model="item.baohanh" />
                          </div>
                        </div>

                        <div class="form-group row">
                          <b class="col-12 col-lg-12 col-form-label">Điều kiện thanh toán:</b>
                          <div class="col-12 col-lg-12 pt-1">
                            <input class="form-control form-control-sm" v-model="item.thanhtoan" />
                          </div>
                        </div>
                      </div>
                      <div class="col-md-12 mb-2">
                        <FormMuahangChitietNcc :index="key"></FormMuahangChitietNcc>
                      </div>
                      <div class="col-md-9">

                      </div>
                      <div class="col-md-3">
                        <div class="row">
                          <b class="col">Tổng giá trị:</b>
                          <span class="col text-right">{{ formatPrice(item.tonggiatri, 0) }} VND</span>
                        </div>
                      </div>

                      <div class="col-12">
                        <b class="">Đính kèm:</b>
                        <div class="custom-file mt-2">
                          <input type="file" class="custom-file-input" :id="'customFile' + key" :multiple="true"
                            :data-key="key" @change="fileChange($event)">
                          <label class="custom-file-label" :for="'customFile' + key">Choose file</label>
                        </div>
                      </div>
                      <div class="col-12 pt-2 list_attachment file-box-content">
                        <div class="file-box" v-for="(item1, key1) in item.dinhkem" :key="key1" :data-key="item1.id">
                          <a :href="item1.url" :download="item1.name" class="download-icon-link">
                            <i class="dripicons-download file-download-icon"></i>
                          </a>
                          <div class="text-center">
                            <i class="far fa-file-pdf text-danger"></i>
                            <h6 class="text-truncate" :title="item1.name">{{ item1.name }}</h6>
                            <div style="cursor: pointer;" @click="xoadinhkem(key1, item)">
                              <i class="fas fa-times-circle text-danger font-16"></i>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </TabPanel>
                </TabView>
              </div>
              <Divider></Divider>
              <div class="flex py-4 gap-2" v-if="readonly == false">
                <Button label="Lập bảng so sánh báo giá" @click="submit1()" size="small" />
              </div>
            </template>
          </StepperPanel>
          <StepperPanel header="So sánh báo giá">
            <template #content="{ prevCallback }">
              <table class="table table-bordered">
                <thead>
                  <tr>
                    <th>Tên hàng hóa</th>
                    <th>Số lượng</th>
                    <th v-for="(item, key) in nccs" :key="key" class="text-center"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      {{ item.ncc.tenncc }}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(item, key) in datatable" :key="key">
                    <td>
                      {{ item.mahh }} - {{ item.tenhh }}
                    </td>
                    <td>
                      {{ item.soluong }} {{ item.dvt }}
                    </td>
                    <td v-for="(item, key1) in nccs" :key="key1" class="text-center"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      {{ formatPrice(item.chitiet[key].thanhtien, 0) }} VNĐ
                    </td>
                  </tr>
                  <tr>
                    <td>Tổng giá trị</td>
                    <td>--</td>
                    <td v-for="(item, key) in nccs" :key="key" class="text-center"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      <b>{{ formatPrice(item.tonggiatri, 0) }} VNĐ</b>
                    </td>
                  </tr>
                  <tr>
                    <td>Đáp ứng các yêu cầu về hàng hóa, dịch vụ </td>
                    <td>--</td>
                    <td v-for="(item, key) in nccs" :key="key" class="text-center"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      <i class="far fa-check-circle text-success" v-if="item.dapung == true"></i>

                    </td>
                  </tr>
                  <tr>
                    <td>Thời gian giao hàng </td>
                    <td>--</td>
                    <td v-for="(item, key) in nccs" :key="key" class="text-center"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      {{ item.thoigiangiaohang }}
                    </td>
                  </tr>
                  <tr>
                    <td>Điều kiện thanh toán</td>
                    <td>--</td>
                    <td v-for="(item, key) in nccs" :key="key" class="text-center"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      {{ item.thanhtoan }}
                    </td>
                  </tr>
                  <tr>
                    <td>Chính sách bảo hành,dịch vụ hậu mãi</td>
                    <td>--</td>
                    <td v-for="(item, key) in nccs" :key="key" class="text-center"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      {{ item.baohanh }}
                    </td>
                  </tr>
                  <tr>
                    <td>Đính kèm </td>
                    <td>--</td>
                    <td v-for="(item, key) in nccs" :key="key" class="text-center"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      <div class="mb-2" v-for="(item1, key1) in item.dinhkem" :key="key1">
                        <a :href="item1.url" :download="model.name"
                          class="download-icon-link d-inline-flex align-items-center" target="_blank">
                          <i class="far fa-file text-danger" style="font-size: 30px; margin-right: 10px;"></i>
                          {{ item1.name }}
                        </a>
                      </div>
                    </td>
                  </tr>
                  <tr>
                    <td>Chọn mua </td>
                    <td>--</td>
                    <td v-for="(item, key) in nccs" :key="key" class="text-center chonmua"
                      :class="{ highlight: model.muahang_chonmua_id == item.id }">
                      <div class="form-check">
                        <input class="form-check-input" type="radio" name="exampleRadios" :id="'exampleRadios' + key"
                          v-model="model.muahang_chonmua_id" :value="item.id">
                        <label class="form-check-label" :for="'exampleRadios' + key">

                        </label>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
              <div class="text-center" v-if="readonly == false">
                <Button label="Xuất PDF" icon="pi pi-file" class="p-button-sm mr-2" @click.prevent="xuatpdf()"></Button>
              </div>
            </template>
          </StepperPanel>
          <StepperPanel header="Trình ký">
            <template #content="{ prevCallback }">
              <div class="row my-5">
                <div class="col-md-12 text-center">
                  <a :href="model.pdf" :download="model.pdf" class="download-icon-link d-inline-flex align-items-center">
                    <i class="far fa-file text-danger" style="font-size: 40px; margin-right: 10px;"></i>
                    {{ model.pdf }}
                  </a>
                  <div class="d-inline-block ml-5" v-if="model.status_id == 8">
                    <a :href="path_esign + '/admin/document/create?queue_id=' + model.esign_id">
                      <Button label="Bắt đầu trình ký" class="p-button-primary p-button-sm mr-2"
                        icon="fas fa-long-arrow-alt-right"></Button>
                    </a>
                  </div>

                  <div class="d-inline-block ml-5" v-else-if="model.status_id == 9">
                    <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                      <Button label="Đang trình ký" class="p-button-warning p-button-sm mr-2"
                        icon="fas fa-spinner fa-spin"></Button>
                    </a>
                  </div>

                  <div class="d-inline-block ml-5" v-else-if="model.status_id == 10">
                    <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                      <Button label="Đã hoàn thành" class="p-button-success p-button-sm mr-2"
                        icon="fas fa-thumbs-up"></Button>
                    </a>
                  </div>

                  <div class="d-inline-block ml-5" v-else-if="model.status_id == 11">
                    <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                      <Button label="Không duyệt" class="p-button-danger p-button-sm mr-2" icon="fas fa-times"></Button>
                    </a>
                  </div>
                </div>
              </div>
            </template>
          </StepperPanel>
        </Stepper>
      </section>
    </div>
  </div>


  <Loading :waiting="waiting"></Loading>
</template>

<script setup>
import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useToast } from "primevue/usetoast";
import Stepper from 'primevue/stepper';
import Divider from 'primevue/divider';
import StepperPanel from 'primevue/stepperpanel';
import TabView from 'primevue/tabview';
import TabPanel from 'primevue/tabpanel';
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import Loading from "../../../components/Loading.vue";
import NccTreeSelect from '../../../components/TreeSelect/NccTreeSelect.vue'
import FormMuahangChitiet from '../../../components/Datatable/FormMuahangChitiet.vue'
import FormMuahangChitietNcc from '../../../components/Datatable/FormMuahangChitietNcc.vue'
import AlertError from "../../../components/AlertError.vue";
import { useRoute, useRouter } from "vue-router";
import muahangApi from "../../../api/muahangApi";
import { useMuahang } from '../../../stores/muahang';

import { storeToRefs } from "pinia";
import { useGeneral } from "../../../stores/general";
import moment from "moment";
import AlertSuccess from "../../../components/AlertSuccess.vue";
import { formatPrice } from "../../../utilities/util";
import { useConfirm } from "primevue/useconfirm";
const confirm = useConfirm();
const readonly = ref(false);
const path_esign = import.meta.env.VITE_ESIGNURL;
const activeStep = ref(0);
const nccmodel = ref();
const active = ref();
const waiting = ref();
const toast = useToast();
const route = useRoute();
const storeMuahang = useMuahang();
const store_general = useGeneral();
const router = useRouter();
const messageError = ref();
const messageSuccess = ref();
const { supliers } = storeToRefs(store_general);
const { model, datatable, list_add, list_update, list_delete, nccs } = storeToRefs(storeMuahang);

const fileChange = (e) => {
  console.log(e.target.files);
  var parents = $(e.target).parents(".custom-file");
  var label = $(".custom-file-label", parents);
  label.text(e.target.files.length + " Files");
}
const addNCC = () => {
  if (!nccmodel.value) {
    alert("Chọn nhà cung cấp!");
    return false;
  }
  var ncc = {};
  var ncc1 = supliers.value.find((item) => item.mancc == nccmodel.value);
  ncc.chitiet = datatable.value.map((item) => {
    delete item.id;
    item.dongia = 0;
    item.thanhtien = 0;
    return item;
  });
  ncc.ncc_id = ncc1.id;
  ncc.ncc = ncc1;
  ncc.muahang_id = model.value.id;
  ncc.thoigiangiaohang = "";
  ncc.baohanh = "";
  ncc.thanhtoan = "";
  ncc.dapung = true;
  ncc.tonggiatri = 0;
  ncc.chonmua = false;
  nccs.value.push(ncc);
  nccmodel.value = null;
}
const removeNCC = (ncc) => {
  // console.log(ncc);
  nccs.value = nccs.value.filter((item) => {
    return ![ncc].includes(item)
  });
}

const xoadinhkem = (key1, item) => {
  // console.log(item.dinhkem[key1]);
  confirm.require({
    message: 'Bạn có chắc muốn xóa?',
    header: 'Xác nhận',
    icon: 'pi pi-exclamation-triangle',
    accept: () => {

      waiting.value = true;
      muahangApi.xoadinhkem({ id: item.dinhkem[key1].id }).then((response) => {
        waiting.value = false;
        messageError.value = "";
        messageSuccess.value = "";
        if (response.success) {
          $(".file-box[data-key='" + item.dinhkem[key1].id + "']").remove();
          toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Xóa đính kèm', life: 3000 });
          delete item.dinhkem[key1];
        } else {
          messageError.value = response.message;
        }

        // console.log(response)
      });


    }
  });

}

const xuatpdf = async () => {
  if (!model.value.name) {
    alert("Chưa nhập tiêu đề!");
    return false;
  }
  if (!model.value.muahang_chonmua_id) {
    alert("Chưa chọn mua nhà cung cấp nào!");
    return false;
  }
  if (!model.value.note) {
    alert("Chưa nhập lý do mua hàng!");
    return false;
  }
  await muahangApi.save(model.value);
  waiting.value = true;
  muahangApi.xuatpdf(model.value.id).then((response) => {
    waiting.value = false;
    messageError.value = "";
    messageSuccess.value = "";
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Xuất file thành công', life: 3000 });
      location.reload();
    } else {
      messageError.value = response.message;
    }

    // console.log(response)
  });
}

const submit = async () => {
  if (!model.value.name) {
    alert("Chưa nhập tiêu đề!");
    return false;
  }

  if (!model.value.note) {
    alert("Chưa nhập lý do mua hàng!");
    return false;
  }
  if (datatable.value.length) {
    for (let product of datatable.value) {
      if (!product.mahh) {
        alert("Chưa chọn Mã NVL!");
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
  var response = await muahangApi.save(model.value);
  waiting.value = false;
  messageError.value = "";
  messageSuccess.value = "";
  if (response.success) {
    toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
    location.reload();
  } else {
    messageError.value = response.message;
  }
  return true;
  // console.log(response)

};

const submit1 = () => {
  if (!nccs.value.length) {
    alert("Chưa chọn nhà cung cấp!");
    return false;
  }
  waiting.value = true;
  var params = { nccs: nccs.value };

  // console.log(params)
  for (var ncc of params.nccs) {
    delete ncc.ncc;
  }
  $(".custom-file-input").each(function (index) {
    console.log(this)
    var files = $(this)[0].files;
    var key = $(this).data("key");
    for (var stt in files) {
      var file = files[stt];
      params["file_" + key + "_" + stt] = file;
    }
  });
  muahangApi.saveNcc(params).then((response) => {
    waiting.value = false;
    messageError.value = "";
    messageSuccess.value = "";
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
      location.reload();
    } else {
      messageError.value = response.message;
    }
    // console.log(response)
  });
};
const baogia = () => {
  waiting.value = true;
  muahangApi.baogia(model.value.id).then((response) => {
    waiting.value = false;
    messageError.value = "";
    messageSuccess.value = "";
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Xuất file thành công', life: 3000 });
      location.reload();
    } else {
      messageError.value = response.message;
    }

    // console.log(response)
  });
}

onMounted(() => {
  muahangApi.get(route.params.id).then((res) => {
    var chitiet = res.chitiet;
    var list_ncc = res.nccs;
    res.date = moment(res.date).format("YYYY-MM-DD");
    delete res.chitiet;
    delete res.nccs;
    model.value = res;
    datatable.value = chitiet;
    nccs.value = list_ncc;
    if (model.value.status_id == 6) {
      activeStep.value = 1;
    } else if (model.value.status_id == 7) {
      activeStep.value = 2;
    } else if ([8, 9, 10, 11].indexOf(model.value.status_id) != -1) {
      activeStep.value = 3;
    }

    if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
      readonly.value = true;
    }
  });
});
</script>
<style scoped>
th.highlight {
  border: 2px solid #04a9c4 !important;
  border-bottom: 2px solid #e0e0e0 !important;
}

td.highlight {
  border-left: 2px solid #04a9c4 !important;
  border-right: 2px solid #04a9c4 !important;
}

td.chonmua.highlight {
  border-bottom: 2px solid #04a9c4 !important;
}
</style>