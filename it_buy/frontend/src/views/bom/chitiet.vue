<template>
  <div class="row clearfix">
    <div class="col-12">
      <form method="POST" id="form">
        <section class="card card-fluid">
          <div class="card-body">
            <div class="row">
              <div class="col-md-3">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label"
                    >Sản phẩm:<i class="text-danger">*</i></b
                  >
                  <div class="col-12 col-lg-12 pt-1">
                    <ProductTreeSelect
                      v-model="model.mahh"
                      @update:modelValue="changeProduct"
                    ></ProductTreeSelect>
                  </div>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label"
                    >Cỡ lô:<i class="text-danger">*</i>
                  </b>
                  <div class="col-12 col-lg-12 pt-1">
                    <InputNumber
                      size="small"
                      :suffix="' ' + model.dvt"
                      v-model="model.colo"
                      class="w-100"
                      inputClass="p-inputtext-sm"
                    ></InputNumber>
                  </div>
                </div>
              </div>
              <div class="col-md-12">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label"
                    >Nguyên vật liệu:<i class="text-danger">*</i></b
                  >
                  <div class="col-12 col-lg-12 pt-1">
                    <FormBomChitiet></FormBomChitiet>
                  </div>
                </div>
              </div>
              <div class="col-md-12 text-center">
                <Button
                  label="Lưu lại"
                  icon="pi pi-save"
                  class="p-button-success p-button-sm mr-2"
                  @click="submit()"
                  :disabled="buttonDisabled"
                ></Button>
              </div>
            </div>
          </div>
        </section>
      </form>
    </div>
  </div>
</template>
    
    <script setup>
import { ref } from "vue";
import { onMounted, computed } from "vue";

import Button from "primevue/button";
import InputNumber from "primevue/inputnumber";
import FormBomChitiet from "../../components/bom/FormBomChitiet.vue";
import { useRoute, useRouter } from "vue-router";
import BomApi from "../../api/BomApi";
import { useBom } from "../../stores/Bom";
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import { rand } from "../../utilities/rand";
import ProductTreeSelect from "../../components/TreeSelect/ProductTreeSelect.vue";
import { useGeneral } from "../../stores/general";
const toast = useToast();
const storeBom = useBom();
const router = useRouter();
const route = useRoute();
const store_general = useGeneral();
const messageError = ref();
const buttonDisabled = ref();
const { model, datatable, list_add, list_update, list_delete } =
  storeToRefs(storeBom);
const { products } = storeToRefs(store_general);
onMounted(async () => {
  await store_general.fetchProducts();
  load_data(route.query.mahh, route.query.colo);
});
const load_data = async (mahh, colo) => {
  var bom = await BomApi.get(mahh, colo);

  datatable.value = bom.details;
  model.value = bom;
};
const changeProduct = (item) => {
  var hh = products.value.find((i) => {
    return i.mahh == item;
  });
  model.value.tenhh = hh.tenhh;
  model.value.dvt = hh.dvt;
  model.value.mahh_goc = hh.mahh_goc;
  model.value.tenhh_goc = hh.tenhh_goc;
  model.value.dvt_goc = hh.dvt;
};
const submit = async () => {
  buttonDisabled.value = true;
  if (!model.value.mahh) {
    alert("Chưa chọn sản phẩm!");
    buttonDisabled.value = false;
    return false;
  }
  if (!model.value.colo) {
    alert("Chưa nhập cỡ lô!");
    buttonDisabled.value = false;
    return false;
  }
  var list_add_thaythe = [];
  var list_update_thaythe = [];
  var list_delete_thaythe = [];

  if (datatable.value.length) {
    for (let product of datatable.value) {
      if (!product.tennvl || !product.manvl) {
        alert("Chưa nhập NVL!");
        buttonDisabled.value = false;
        return false;
      }
      if (!product.dvtnvl) {
        alert("Chưa nhập đơn vị tính!");
        buttonDisabled.value = false;
        return false;
      }
      product.mahh = model.value.mahh;
      product.tenhh = model.value.tenhh;
      product.dvt = model.value.dvt;
      product.mahh_goc = model.value.mahh_goc;
      product.tenhh_goc = model.value.tenhh_goc;
      product.colo = model.value.colo;
      product.dvt_goc = model.value.dvt_goc;

      if (product.thaythe.length) {
        for (let th of product.thaythe) {
          if (!th.tennvl || !th.manvl) {
            alert("Chưa nhập NVL thay thế!");
            buttonDisabled.value = false;
            return false;
          }
          if (!th.dvtnvl) {
            alert("Chưa nhập đơn vị tính NVL thay thế!");
            buttonDisabled.value = false;
            return false;
          }
          var nvl_thaythe = {};
          nvl_thaythe.mahh = model.value.mahh;
          nvl_thaythe.colo = model.value.colo;
          nvl_thaythe.manvl = product.manvl;
          nvl_thaythe.stt_thaythe = th.stt;
          nvl_thaythe.manvl_thaythe = th.manvl;
          nvl_thaythe.tennvl_thaythe = th.tennvl;
          nvl_thaythe.dvtnvl_thaythe = th.dvtnvl;
          nvl_thaythe.me_thaythe = th.me;
          nvl_thaythe.soluong_thaythe = th.soluong;
          if (th.deleted && !th.ids) {
            list_delete_thaythe.push(nvl_thaythe);
          } else if (th.ids && !th.deleted) {
            list_add_thaythe.push(nvl_thaythe);
          } else if (!th.deleted && !th.ids) {
            list_update_thaythe.push(nvl_thaythe);
          }
        }
      }
    }
  } else {
    alert("Chưa nhập nguyên liệu!");
    buttonDisabled.value = false;
    return false;
  }

  var params = {};
  params.list_add = list_add.value;
  params.list_update = list_update.value;
  params.list_delete = list_delete.value.map((product) => {
    product.mahh = model.value.mahh;
    product.tenhh = model.value.tenhh;
    product.dvt = model.value.dvt;
    product.mahh_goc = model.value.mahh_goc;
    product.tenhh_goc = model.value.tenhh_goc;
    product.colo = model.value.colo;
    product.dvt_goc = model.value.dvt_goc;
    return product;
  });

  var params_thaythe = {};
  params_thaythe.list_add = list_add_thaythe;
  params_thaythe.list_update = list_update_thaythe;
  params_thaythe.list_delete = list_delete_thaythe;

  var all = [BomApi.save(params), BomApi.saveThaythe(params_thaythe)];
  await Promise.all(all);

  toast.add({
    severity: "success",
    summary: "Thành công!",
    detail: "Tạo thành công",
    life: 3000,
  });
  router.push("/bom");
  // console.log(response)
};
</script>
    