<template>
  <div class="col-12 text-center" v-if="readonly == false">
    <div class="row">
      <div class="col-4">
        <div class="d-flex align-items-center">
          <NccTreeSelect v-model="nccmodel"></NccTreeSelect>
          <span class="fas fa-plus text-success ml-3" style="cursor: pointer" @click="openNew()"></span>
        </div>
        <PopupAdd @save="addNewNhacc"></PopupAdd>
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
          {{ item.ncc?.tenncc }}
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
                <div class="custom-control custom-switch switch-success" style="height: 36px">
                  <input type="checkbox" :id="'dapung_' + key" class="custom-control-input" v-model="item.dapung"
                    :disabled="readonly" />
                  <label :for="'dapung_' + key" class="custom-control-label"></label>
                </div>
              </div>
            </div>
            <div class="form-group row">
              <b class="col-12 col-lg-12 col-form-label">Tiền tệ:</b>
              <div class="col-12 col-lg-12 pt-1">
                {{ item.tiente }}
                <i class="fas fa-sync-alt" style="cursor: pointer" @click="openOp($event, item)"
                  v-if="readonly == false"></i>
              </div>
            </div>

            <div class="form-group row">
              <b class="col-12 col-lg-12 col-form-label">Bao gồm VAT:</b>
              <div class="col-12 col-lg-12 pt-1">
                <div class="custom-control custom-switch switch-success" style="height: 36px">
                  <input type="checkbox" :id="'novat_' + key" class="custom-control-input" v-model="item.is_vat"
                    :disabled="readonly" />
                  <label :for="'novat_' + key" class="custom-control-label"></label>
                </div>
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-group row">
              <b class="col-12 col-lg-12 col-form-label">Chính sách bảo hành:</b>
              <div class="col-12 col-lg-12 pt-1">
                <input class="form-control form-control-sm" v-model="item.baohanh" :disabled="readonly" />
              </div>
            </div>
            <div class="form-group row">
              <b class="col-12 col-lg-12 col-form-label">Thời gian giao hàng:</b>
              <div class="col-12 col-lg-12 pt-1">
                <input class="form-control form-control-sm" v-model="item.thoigiangiaohang" :disabled="readonly" />
              </div>
            </div>
            <div class="form-group row">
              <b class="col-12 col-lg-12 col-form-label">Điều kiện thanh toán:</b>
              <div class="col-12 col-lg-12 pt-1">
                <input class="form-control form-control-sm" v-model="item.thanhtoan" :disabled="readonly" />
              </div>
              <div class="col-12 col-lg-12 pt-3" v-if="readonly == false && model.type_id == 3">
                <Button label="Lấy đơn giá" icon="pi pi-plus" size="small" @click="laydongiahoachat(item, key)"
                  class="float-right"></Button>
              </div>
            </div>
          </div>
          <div class="col-md-12 mb-2">
            <FormMuahangChitietNcc :index="key" :ref="el => (formMuahangRefs[key] = el)"></FormMuahangChitietNcc>
          </div>

          <div class="row col-12 mt-2">
            <div class="col-md-8"></div>

            <div class="col-md-4" v-if="item.is_vat">
              <div class="row">
                <b class="col">Thành tiền (Chưa VAT):</b>
                <span class="col text-right">
                  <InputNumber v-model="item.thanhtien" class="p-inputtext-sm" :suffix="' ' + item.tiente"
                    :disabled="readonly || !item.is_edit" :maxFractionDigits="2" />
                </span>
              </div>
            </div>
          </div>

          <div class="row col-12 mt-2">
            <div class="col-md-8"></div>

            <div class="col-md-4" v-if="item.is_vat">
              <div class="row">
                <b class="col">Tiền VAT:</b>
                <span class="col text-right">
                  <InputNumber v-model="item.tienvat" class="p-inputtext-sm" :suffix="' ' + item.tiente"
                    :disabled="readonly || !item.is_edit" :maxFractionDigits="2" />
                </span>
              </div>
            </div>
          </div>

          <div class="row col-12 mt-2">
            <div class="col-md-8"></div>

            <div class="col-md-4">
              <div class="row">
                <b class="col">Thành tiền:</b>
                <span class="col text-right">
                  <InputNumber v-model="item.thanhtien_vat" class="p-inputtext-sm" :suffix="' ' + item.tiente"
                    :disabled="readonly || !item.is_edit" :maxFractionDigits="2" />
                </span>
              </div>
            </div>
          </div>

          <div class="row col-12 mt-2">
            <div class="col-md-8"></div>

            <div class="col-md-4">
              <div class="row">
                <b class="col">Phí giao hàng:</b>
                <span class="col text-right">
                  <InputNumber v-model="item.phigiaohang" class="p-inputtext-sm" :suffix="' ' + item.tiente"
                    @update:modelValue="changetien(key)" :disabled="readonly" :maxFractionDigits="2" />
                </span>
              </div>
            </div>
          </div>
          <div class="row col-12 mt-2">
            <div class="col-md-8"></div>

            <div class="col-md-4">
              <div class="row">
                <b class="col">Chiết khấu:</b>
                <span class="col text-right">
                  <InputNumber v-model="item.ck" class="p-inputtext-sm" :suffix="' ' + item.tiente"
                    @update:modelValue="changetien(key)" :disabled="readonly" :maxFractionDigits="2" />
                </span>
              </div>
            </div>
          </div>
          <div class="row col-12 mt-2">
            <div class="col-md-8"></div>

            <div class="col-md-4">
              <div class="row">
                <b class="col">Tổng giá trị:
                  <span class="pi pi-pencil ml-2" style="cursor: pointer" v-if="readonly == false"
                    @click="item.is_edit = true"></span></b>
                <span class="col text-right">
                  <InputNumber v-model="item.tonggiatri" class="p-inputtext-sm" :suffix="' ' + item.tiente"
                    :disabled="readonly || !item.is_edit" :maxFractionDigits="2" />
                </span>
              </div>
            </div>
          </div>
          <div class="col-12" v-if="readonly == false">
            <b class="">Đính kèm:</b>
            <div class="custom-file mt-2">
              <input type="file" class="custom-file-input" :id="'customFile' + key" :multiple="true" :data-key="key"
                @change="fileChange($event)" />
              <label class="custom-file-label" :for="'customFile' + key">Choose file</label>
            </div>
          </div>
          <div class="col-12 pt-2 list_attachment file-box-content">
            <div class="file-box" v-for="(item1, key1) in item.dinhkem" :key="key1" :data-key="item1.id">
              <a :href="item1.url" target="_blank" :download="download(item1.name)" class="download-icon-link">
                <i class="dripicons-download file-download-icon"></i>

                <div class="text-center">
                  <i class="far fa-file-pdf text-danger"></i>
                  <h6 class="text-truncate" :title="item1.name">
                    {{ item1.name }}
                  </h6>
                  <div style="cursor: pointer" @click.prevent="xoadinhkemncc(key1, item)" v-if="readonly == false">
                    <i class="fas fa-times-circle text-danger font-16"></i>
                  </div>
                </div>
              </a>
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
  <OverlayPanel ref="op">
    <div>
      Qui đổi tạm tính từ 1
      <input class="form-control form-control-sm d-inline-block" style="width: 60px" v-model="editingRow.tiente" />
      =
      <input class="form-control form-control-sm d-inline-block" style="width: 60px" v-model="editingRow.quidoi" />
      <b>VND</b>
    </div>
  </OverlayPanel>
</template>
<script setup>
import { onMounted, ref } from "vue";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import OverlayPanel from "primevue/overlaypanel";
import Divider from "primevue/divider";
import Button from "primevue/button";
import TabView from "primevue/tabview";
import TabPanel from "primevue/tabpanel";
import InputNumber from "primevue/inputnumber";
import { useToast } from "primevue/usetoast";
import { useGeneral } from "../../stores/general";
import PopupAdd from "../supplier/PopupAdd.vue";
import NccTreeSelect from "../TreeSelect/NccTreeSelect.vue";
import FormMuahangChitietNcc from "../Datatable/FormMuahangChitietNcc.vue";
import { useSupplier } from "../../stores/supplier";
import { rand } from "../../utilities/rand";
import { download, formatPrice } from "../../utilities/util";
import { useConfirm } from "primevue/useconfirm";
const formMuahangRefs = ref([]);
const confirm = useConfirm();
const toast = useToast();
const store_muahang = useMuahang();
const store_general = useGeneral();
const { supliers } = storeToRefs(store_general);
const store_supplier = useSupplier();
const RefSupplier = storeToRefs(store_supplier);
const modelNhacc = RefSupplier.model;
const { headerForm, visibleDialog } = RefSupplier;
const { model, list_add, list_update, list_delete, datatable, nccs, waiting } =
  storeToRefs(store_muahang);
const active = ref();
const nccmodel = ref();

const editingRow = ref();
const op = ref();
const openOp = (event, row) => {
  editingRow.value = row;
  op.value.toggle(event);
};
const openNew = () => {
  modelNhacc.value = {};
  headerForm.value = "Tạo mới";
  visibleDialog.value = true;
};
const fileChange = (e) => {
  var parents = $(e.target).parents(".custom-file");
  var label = $(".custom-file-label", parents);
  label.text(e.target.files.length + " Files");
};
const addNewNhacc = () => {
  store_general.fetchNhacc(false);
};
const addNCC = () => {
  if (!nccmodel.value) {
    alert("Chọn nhà cung cấp!");
    return false;
  }
  var ncc = {};
  var ncc1 = supliers.value.find((item) => item.id == nccmodel.value);
  var chitiet = [];
  for (let item of datatable.value) {
    let obj = Object.assign({}, item);
    obj.muahang_chitiet_id = obj.id;
    obj.dongia = 0;
    obj.thanhtien = 0;
    obj.thanhtien_vat = 0;
    obj.vat = 0;
    delete obj.id;
    delete obj.muahang_ncc_chitiet;
    delete obj.user_nhanhang;
    chitiet.push(obj);
  }
  ncc.chitiet = chitiet;
  ncc.ids = rand();
  ncc.ncc_id = ncc1.id;
  ncc.ncc = ncc1;
  ncc.muahang_id = model.value.id;
  ncc.thoigiangiaohang = "";
  ncc.baohanh = "";
  ncc.thanhtoan = "";
  ncc.dapung = true;
  ncc.is_vat = true;
  ncc.tienvat = 0;
  ncc.phigiaohang = 0;
  ncc.ck = 0;
  ncc.thanhtien = 0;
  ncc.thanhtien_vat = 0;
  ncc.tonggiatri = 0;
  ncc.chonmua = false;
  ncc.tiente = "VND";
  ncc.quidoi = 1;
  nccs.value.push(ncc);
  // console.log(nccs.value);
  active.value = nccs.value.length - 1;
  nccmodel.value = null;
};

const changetien = (index) => {
  var ncc = nccs.value[index];
  var tonggiatri = ncc.thanhtien + ncc.tienvat + ncc.phigiaohang - ncc.ck;
  ncc.tonggiatri = tonggiatri;
};
const removeNCC = (ncc) => {
  // console.log(ncc);
  confirm.require({
    message: "Bạn có chắc muốn xóa?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      if (ncc.id > 0) {
        muahangApi.xoancc({ id: ncc.id });
      }
      nccs.value = nccs.value.filter((item) => {
        return ![ncc].includes(item);
      });
    },
  });
};

const xoadinhkemncc = (key1, item) => {
  // console.log(item.dinhkem[key1]);
  confirm.require({
    message: "Bạn có chắc muốn xóa?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      waiting.value = true;
      muahangApi
        .xoadinhkemncc({ id: item.dinhkem[key1].id })
        .then((response) => {
          waiting.value = false;
          if (response.success) {
            $(
              ".list_attachment .file-box[data-key='" +
              item.dinhkem[key1].id +
              "']"
            ).remove();
            toast.add({
              severity: "success",
              summary: "Thành công!",
              detail: "Xóa đính kèm",
              life: 3000,
            });
            delete item.dinhkem[key1];
          }

          // console.log(response)
        });
    },
  });
};

const submit1 = () => {
  if (!nccs.value.length) {
    alert("Chưa chọn nhà cung cấp!");
    return false;
  }
  var params = { nccs: nccs.value };

  // console.log(params)
  for (var ncc of params.nccs) {
    delete ncc.ncc;
    delete ncc.dinhkem;
    if (ncc.phigiaohang == null) {
      alert("Chưa nhập phí giao hàng!");
      return false;
    }
    if (ncc.ck == null) {
      alert("Chưa nhập chiết khấu!");
      return false;
    }
    for (var c of ncc.chitiet) {
      delete c.muahang_chitiet;
      if (c.dongia == null) {
        alert("Chưa nhập đơn giá!");
        return false;
      }
      if (c.vat == null) {
        alert("Chưa nhập vat!");
        return false;
      }
    }
  }
  $(".custom-file-input").each(function (index) {
    // console.log(this)
    var files = $(this)[0].files;
    var key = $(this).data("key");
    for (var stt = 0; stt < files.length; stt++) {
      var file = files[stt];
      params["file_" + key + "_" + stt] = file;
    }
  });
  ////

  waiting.value = true;
  if (params.nccs[0].chitiet.length > 10) {
    var PromiseAll = [];
    for (var ncc of params.nccs) {
      // console.log(ncc);
      var promise = muahangApi.saveNcc({ nccs: [ncc] });
      PromiseAll.push(promise);
    }
    Promise.all(PromiseAll).then((response) => {
      waiting.value = false;
      toast.add({
        severity: "success",
        summary: "Thành công!",
        detail: "Thay đổi thành công",
        life: 3000,
      });
      location.reload();
    });
  } else {
    muahangApi.saveNcc(params).then((response) => {
      waiting.value = false;
      if (response.success) {
        toast.add({
          severity: "success",
          summary: "Thành công!",
          detail: "Thay đổi thành công",
          life: 3000,
        });
        location.reload();
        // store_muahang.load_data(model.value.id);
      }
      // console.log(response)
    });
  }
};
const laydongiahoachat = async (item, index) => {
  // console.log(item);
  if (!item.ncc_id) {
    alert("Chưa chọn nhà cung cấp!");
    return false;
  }
  var params = {
    mancc: item.ncc.mancc,
    muahang_id: model.value.id,
  };
  waiting.value = true;
  var response = await muahangApi.laydongiahoachat(params);
  waiting.value = false;
  for (var key in response) {
    var dongia = response[key];
    var findIndex = item.chitiet.findIndex((item1) => item1.muahang_chitiet_id == key);
    // console.log(findIndex);
    if (findIndex != -1) {
      item.chitiet[findIndex].dongia = dongia;
    }
  }
  // console.log(item);
  // console.log(formMuahangRefs.value[index]);
  formMuahangRefs.value[index].changeDongia();
};
const readonly = ref(false);
onMounted(() => {
  if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
    readonly.value = true;
  }
});
</script>

<style>
.p-inputnumber-input {
  text-align: right;
}
</style>
