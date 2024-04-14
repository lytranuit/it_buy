<template>
  <div v-if="model.status_id > 6">
    <table class="table table-bordered">
      <thead>
        <tr>
          <th>Tên hàng hóa</th>
          <th>Số lượng</th>
          <th v-for="(item, key) in nccs" :key="key" class="text-center"
            :class="{ highlight: model.muahang_chonmua_id == item.id }">
            {{ item.ncc?.tenncc }}
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
            {{ formatPrice(item.chitiet[key].thanhtien_vat, 0) }} {{ item.tiente }}
          </td>
        </tr>
        <tr>
          <td>Tổng giá trị</td>
          <td>--</td>
          <td v-for="(item, key) in nccs" :key="key" class="text-center"
            :class="{ highlight: model.muahang_chonmua_id == item.id }">
            <b>{{ formatPrice(item.tonggiatri, 0) }} {{ item.tiente }}</b>
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
              <a :href="item1.url" :download="download(model.name)"
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
            <div class="form-check" v-if="readonly == false">
              <input class="form-check-input" type="radio" name="exampleRadios" :id="'exampleRadios' + key"
                v-model="model.muahang_chonmua_id" :value="item.id">
              <label class="form-check-label" :for="'exampleRadios' + key">

              </label>
            </div>
            <div v-else>
              <i class="fas fa-check text-success" v-if="model.muahang_chonmua_id == item.id"></i>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div class="col-md-12">
      <div class="form-group row">
        <b class="col-12 col-lg-12 col-form-label">Lưu ý / Lý do chọn mua:</b>
        <div class="col-12 col-lg-12 pt-1">
          <textarea class="form-control" v-model="model.note_chonmua" :disabled="readonly"></textarea>
        </div>
      </div>
    </div>
    <div class="text-center" v-if="readonly == false">
      <Button label="Xem trước" icon="pi pi-eye" class="p-button-info p-button-sm mr-2"
        @click.prevent="view()"></Button>
      <Button label="Xuất PDF và trình ký" icon="pi pi-file" class="p-button-sm mr-2"
        @click.prevent="xuatpdf()"></Button>
    </div>
  </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import Button from 'primevue/button';
import { useToast } from "primevue/usetoast";
import { download, formatPrice } from "../../utilities/util";
const toast = useToast();
const store_muahang = useMuahang();
const { model, datatable, nccs, waiting } = storeToRefs(store_muahang);

const readonly = ref(false);

const view = async () => {
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
  muahangApi.xuatpdf(model.value.id, true).then((response) => {
    waiting.value = false;
    if (response.success) {
      window.open(response.link, '_blank').focus();
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
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Xuất file thành công', life: 3000 });
      // location.reload();
      store_muahang.load_data(model.value.id);
    }
  });
}
onMounted(() => {
  if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
    readonly.value = true;
  }
})
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
