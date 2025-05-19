<template>
  <div class="row">
    <div class="col-12 d-flex">
      <span>Lịch sử mua hàng của mã {{ to }}</span>
      <div class="custom-control custom-switch switch-success ml-auto" style="height: 36px">
        <input type="checkbox" :id="'is_sametype'" class="custom-control-input" v-model="is_sametype" />
        <label :for="'is_sametype'" class="custom-control-label">Bao gồm hàng hóa cùng loại</label>
      </div>
    </div>
    <div class="col-12 mt-2">
      <table class="table table-border">
        <thead>
          <tr>
            <th>Đề nghị mua hàng</th>
            <th>Nhà cung cấp</th>
            <th>Số lượng</th>
            <th>Đơn giá</th>
            <th>Thành tiền</th>
            <th>Ngày giao/nhận</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in history" :key="item.id">
            <th>
              <RouterLink :to="'/muahang/edit/' + item.muahang.id">{{ item.muahang.code }} - {{ item.muahang.name }}
              </RouterLink>
            </th>
            <th>{{ item.ncc?.tenncc }}</th>
            <th>{{ item.soluong }}</th>
            <th>{{ formatPrice(item.dongia, 2) }} {{ item.tiente }}</th>
            <th>{{ formatPrice(item.thanhtien, 2) }} {{ item.tiente }}</th>
            <th>{{ formatDate(item.muahang?.date) }}</th>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import materialApi from "../../api/materialApi";
import { useMaterials } from "../../stores/materials";
import muahangApi from "../../api/muahangApi";
import { formatDate, formatPrice } from "../../utilities/util";
const props = defineProps({
  mahh: {
    type: String
  },
});
const store_materials = useMaterials();
const { model } = storeToRefs(store_materials);
const history = ref([]);
const to = ref();
const is_sametype = ref(false);
const getHistory = async () => {
  var res = await muahangApi.getHistory(props.mahh, is_sametype.value);
  // console.log(res);
  to.value = res.to;
  history.value = res.data;
};
onMounted(() => {
  getHistory();
});
</script>