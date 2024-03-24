<template>
  <div class="row my-5">
    <div class="col-md-4 text-center">
      <h3>Kiểm tra thông tin</h3>
      <a href="#" @click.prevent="tabviewActive = 1">-> Files</a>
    </div>
    <div class="col-md-4">
      <b class="">Ủy nhiệm chi:<span class="text-danger">*</span></b>
      <div class="custom-file mt-2" v-if="model.is_dathang == true && !model.is_thanhtoan && is_Ketoan">
        <input type="file" class="uynhiemchi-file-input" :id="'uynhiemchi'" :multiple="true"
          @change="fileChange($event)">
        <label class="custom-file-label" :for="'uynhiemchi'">Choose file</label>
      </div>
      <div class="mt-2 list_uynhiemchi file-box-content" v-else>
        <div class="file-box" v-for="(item1, key1) in list_uynhiemchi" :key="key1" :data-key="item1.id">
          <a :href="item1.url" :download="item1.name" class="download-icon-link">
            <i class="dripicons-download file-download-icon"></i>
          </a>
          <div class="text-center">
            <i class="far fa-file-pdf text-danger"></i>
            <h6 class="text-truncate" :title="item1.name">{{ item1.name }}</h6>
          </div>
        </div>
      </div>
    </div>

    <div class="col-md-4 align-self-center" v-if="model.is_dathang == true && !model.is_thanhtoan">
      <Button label="Thông báo thanh toán" icon="far fa-paper-plane" size="small" @click.prevent="thongbaothanhtoan()">
      </Button>
    </div>
    <div class="col-md-12 mt-3" v-if="model.is_dathang == true && !model.is_thanhtoan && is_Ketoan">
      <Button label="Hoàn thành thanh toán" icon="fas fa-long-arrow-alt-down" class="p-button-success p-button-sm mr-2"
        @click.prevent="thanhtoan()">
      </Button>
    </div>
  </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import Button from 'primevue/button';
import { useAuth } from "../../stores/auth";
import { useToast } from "primevue/usetoast";
const toast = useToast();
const store_muahang = useMuahang();
const { model, tabviewActive, waiting, list_uynhiemchi } = storeToRefs(store_muahang);

const store = useAuth();
const { is_admin, is_Cungung, is_Ketoan } = storeToRefs(store);
const emit = defineEmits(["next"]);

const fileChange = (e) => {
  var parents = $(e.target).parents(".custom-file");
  var label = $(".custom-file-label", parents);
  label.text(e.target.files.length + " Files");
}
const thanhtoan = async () => {
  var params = {};
  params.muahang_id = model.value.id;
  var files = $(".uynhiemchi-file-input")[0].files;
  if (!files.length) {
    alert("Chưa upload ủy nhiệm chi!");
    return false;
  }
  for (var stt in files) {
    var file = files[stt];
    params["file_" + stt] = file;
  }
  waiting.value = true;
  await muahangApi.saveUynhiemchi(params);
  model.value.is_thanhtoan = true;
  await muahangApi.save(model.value);
  store_muahang.load_data(model.value.id);
  waiting.value = false;
  emit("next");
}

const thongbaothanhtoan = async () => {
  // visibleDialog.value = false;
  var res = await muahangApi.thongbaothanhtoan({ muahang_id: model.value.id });
  if (res.success) {
    toast.add({
      severity: "success",
      summary: "Thành công",
      detail: "Thành công",
      life: 3000,
    });
  }
}
onMounted(() => {

})
</script>

<style lang="scss"></style>
