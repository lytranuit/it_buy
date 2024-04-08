<template>
  <div class="row clearfix">
    <div class="col-12">
      <h5 class="card-header drag-handle">
        Nhận hàng
      </h5>
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
          <div class="row m-0">
            <div class="col-md-4 form-group">
              <b class="">Ngày giao hàng dự kiến:</b>
              <div class="mt-2">
                <Calendar :modelValue="formatDate(model.date)" dateFormat="yy-mm-dd" class="date-custom"
                  :manualInput="false" showIcon :minDate="minDate" :readonly="true" />
              </div>
            </div>
            <div class="col-md-4 form-group">
              <b class="">Nhà cung cấp:</b>
              <div class="mt-2">
                <InputText :modelValue="model?.muahang_chonmua?.ncc?.tenncc" :readonly="true" size="small"
                  class="form-control"></InputText>
              </div>
            </div>
            <div class="col-md-4 form-group">
              <b class="">Mã đặt hàng:</b>
              <div class="mt-2">
                <InputText :modelValue="model?.code" :readonly="true" size="small" class="form-control"></InputText>
              </div>
            </div>
            <div class="col-md-12 mb-2">
              <b class="">Hàng hóa:</b>
              <FormMuahangNhanhang :editable="!model.is_nhanhang"></FormMuahangNhanhang>
            </div>
            <div class="col-md-12 text-center mt-3" v-if="!model.is_nhanhang">
              <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
                @click.prevent="savenhanhang()"></Button>
            </div>
          </div>
        </div>
      </section>
    </div>

    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>
import { onMounted, ref, computed, watch } from "vue";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import { useConfirm } from "primevue/useconfirm";
import Loading from "../../../components/Loading.vue";
import muahangApi from "../../../api/muahangApi";
import { useRoute } from "vue-router";
import { useMuahang } from "../../../stores/muahang";
import { storeToRefs } from "pinia";
import moment from "moment";
import { formatDate } from "../../../utilities/util";
import FormMuahangNhanhang from "../../../components/Datatable/FormMuahangNhanhang.vue";
import { useToast } from "primevue/usetoast";
const minDate = ref(new Date());
const confirm = useConfirm();
const toast = useToast();
const route = useRoute();
const storeMuahang = useMuahang();
const { model, waiting, datatable, nccs } = storeToRefs(storeMuahang);
const load_data = async (id) => {
  var res = await muahangApi.get(id);
  var chitiet = res.chitiet;
  var list_ncc = res.nccs;
  res.date = res.date ? moment(res.date).format("YYYY-MM-DD") : null;
  delete res.chitiet;
  delete res.nccs;
  delete res.user_created_by;
  model.value = res;
  datatable.value = chitiet;
  nccs.value = list_ncc;
};

const savenhanhang = () => {
  // console.log(list_nhanhang);
  var items = [];

  for (var item of datatable.value) {
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
    delete item.muahang_ncc_chitiet;
    items.push(item);
  }

  // console.log(items);
  // return false;
  muahangApi.savenhanhang({ muahang_id: model.value.id, list: items }).then((response) => {
    waiting.value = false;
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
      location.reload();
    }
  });
}
onMounted(() => {
  load_data(route.params.id);
});
</script>
