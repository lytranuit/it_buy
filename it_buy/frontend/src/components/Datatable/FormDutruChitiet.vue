<template>
  <div id="TableDutruChitiet">
    <DataTable
      showGridlines
      :value="datatable"
      ref="dt"
      class="p-datatable-ct"
      :rowHover="true"
      :loading="loading"
      responsiveLayout="scroll"
      :resizableColumns="true"
      columnResizeMode="expand"
      v-model:selection="selected"
      :rowClass="rowClass"
    >
      <template #header>
        <div
          class="d-inline-flex"
          style="width: 200px"
          v-if="model.status_id == 1"
        >
          <Button
            label="Thêm"
            icon="pi pi-plus"
            class="p-button-success p-button-sm mr-2"
            @click="addRow"
          ></Button>
          <Button
            label="Xóa"
            icon="pi pi-trash"
            class="p-button-danger p-button-sm"
            :disabled="!selected || !selected.length"
            @click="confirmDeleteSelected"
          ></Button>
        </div>
        <div class="d-inline-flex float-right"></div>
      </template>

      <template #empty>
        <div class="text-center">Không có dữ liệu.</div>
      </template>
      <Column selectionMode="multiple" v-if="model.status_id == 1"></Column>
      <Column
        v-for="col in selectedColumns"
        :field="col.data"
        :header="col.label"
        :key="col.data"
        :showFilterMatchModes="false"
        :class="col.data"
      >
        <template #body="slotProps">
          <template v-if="col.data == 'tenhh' && model.status_id == 1">
            <div class="d-flex align-items-center">
              <material-auto-complete
                v-model="slotProps.data[col.data]"
                :disabled="slotProps.data['mahh'] != null"
                :type_id="model.type_id"
                @item-select="select($event, slotProps.data)"
              >
              </material-auto-complete>
              <!-- <span class="fas fa-plus ml-3 text-success" style="cursor: pointer;"
                                @click="openNew(slotProps.index)" v-if="model.type_id != 1"></span> -->
            </div>
          </template>
          <template v-else-if="col.data == 'grade' && model.status_id == 1">
            <select
              class="form-control form-control-sm"
              v-model="slotProps.data[col.data]"
            >
              <option value="Dược phẩm">Dược phẩm</option>
              <option value="Mỹ phẩm">Mỹ phẩm</option>
              <option value="Thực phẩm">Thực phẩm</option>
            </select>
          </template>
          <template
            v-else-if="col.data == 'list_dangbaoche' && model.status_id == 1"
          >
            <DangbaocheTreeSelect
              v-model="slotProps.data.list_dangbaoche"
              @update:modelValue="changeDangbaoche($event, slotProps.data)"
              :multiple="true"
            ></DangbaocheTreeSelect>
          </template>
          <template v-else-if="col.data == 'list_sp' && model.status_id == 1">
            <ProductTreeSelect
              v-model="slotProps.data.list_sp"
              @update:modelValue="changeProduct($event, slotProps.data)"
              :multiple="true"
            ></ProductTreeSelect>
          </template>
          <template
            v-else-if="
              (col.data == 'dvt' || col.data == 'masothietke') &&
              model.status_id == 1
            "
          >
            <input
              v-model="slotProps.data[col.data]"
              class="p-inputtext p-inputtext-sm"
              required
            />
          </template>
          <template v-else-if="col.data == 'is_new' && model.status_id == 1">
            <input-switch v-model="slotProps.data[col.data]" />
          </template>
          <template v-else-if="col.data == 'is_new'">
            <span>{{
              slotProps.data[col.data] == false ? "Active" : "Unactive"
            }}</span>
          </template>
          <template v-else-if="col.data == 'nhasx' && model.status_id == 1">
            <NsxTreeSelect
              v-model="slotProps.data['mansx']"
              :required="true"
              :useID="false"
              @update:modelValue="changeProducer(slotProps.data)"
            >
            </NsxTreeSelect>
          </template>
          <template v-else-if="col.data == 'soluong' && model.status_id == 1">
            <input-number
              v-model="slotProps.data[col.data]"
              class="p-inputtext-sm"
              :maxFractionDigits="2"
            />
          </template>

          <template v-else-if="col.data == 'note' && model.status_id == 1">
            <textarea v-model="slotProps.data[col.data]" class="form-control" />
          </template>
          <template v-else-if="col.data == 'date' && model.status_id == 1">
            <Calendar
              v-model="slotProps.data.date"
              dateFormat="yy-mm-dd"
              class="date-custom"
              :manualInput="false"
              showIcon
            />
          </template>
          <template v-else-if="col.data == 'dinhkem' && model.status_id == 1">
            <div class="custom-file mt-2">
              <input
                type="file"
                class="hinhanh-file-input"
                :id="'hinhanhFile' + slotProps.data['stt']"
                :multiple="true"
                :data-key="slotProps.data['stt']"
                @change="fileChange($event)"
              />
              <label
                class="custom-file-label"
                :for="'hinhanhFile' + slotProps.data['stt']"
                >Choose file</label
              >
            </div>
            <div
              class="mt-2 dinhkemchitiet"
              v-for="(item, index) in slotProps.data['dinhkem']"
              :key="index"
            >
              <a
                target="_blank"
                :href="item.url"
                class="text-blue"
                :download="download(item.name)"
                >{{ item.name }}</a
              ><i
                class="text-danger fas fa-trash ml-2"
                style="cursor: pointer"
                @click="xoachitietdinhkem(index, slotProps.data['dinhkem'])"
              ></i>
            </div>
          </template>

          <template v-else-if="col.data == 'dinhkem'">
            <div
              class="mt-2 dinhkemchitiet"
              v-for="(item, index) in slotProps.data['dinhkem']"
              :key="index"
            >
              <a
                target="_blank"
                :href="item.url"
                class="text-blue"
                :download="download(item.name)"
                >{{ item.name }}</a
              >
            </div>
          </template>

          <template v-else-if="col.data == 'tenhh' && model.status_id != 1">
            {{ label(slotProps.data) }}
          </template>
          <template v-else-if="col.data == 'list_sp' && model.status_id != 1">
            {{ slotProps.data.tensp }}
          </template>
          <template
            v-else-if="col.data == 'list_dangbaoche' && model.status_id != 1"
          >
            {{ slotProps.data.dangbaoche }}
          </template>
          <template v-else-if="col.data == 'date' && model.status_id != 1">
            {{ formatDate(slotProps.data.date) }}
          </template>
          <template v-else>
            {{ slotProps.data[col.data] }}
          </template>
        </template>
      </Column>
      <Column
        header="Hủy"
        field="action"
        key="action"
        v-if="model.status_id == 4"
      >
        <template #body="slotProps">
          <a
            href="#"
            class="text-danger"
            @click="huyitem(slotProps.data)"
            v-if="slotProps.data['can_huy'] && user.id == model.created_by"
            ><i class="fas fa-trash"></i> Hủy</a
          >
          <div v-else-if="slotProps.data['date_huy']">
            <div>{{ slotProps.data.note_huy }}</div>
            <div class="small">
              Hủy lúc
              <i>{{
                formatDate(slotProps.data.date_huy, "YYYY-MM-DD HH:mm:ss")
              }}</i>
            </div>
          </div>
        </template>
      </Column>
    </DataTable>
  </div>
  <Dialog
    v-model:visible="visible"
    modal
    header="Lý do hủy"
    style="width: 50vw"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }"
  >
    <textarea
      class="form-control form-control-sm"
      v-model="editRow.note_huy"
      rows="5"
    ></textarea>

    <div class="d-flex justify-content-center mt-2">
      <Button
        type="button"
        label="Cancel"
        severity="secondary"
        @click="visible = false"
        size="small"
        class="mr-2"
      ></Button>
      <Button
        type="button"
        label="Save"
        severity="danger"
        @click="HuyChitiet"
        size="small"
      ></Button>
    </div>
  </Dialog>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputSwitch from "primevue/inputswitch";
import InputNumber from "primevue/inputnumber";
import { storeToRefs } from "pinia";

import { useConfirm } from "primevue/useconfirm";
import { rand } from "../../utilities/rand";
import { useAuth } from "../../stores/auth";
import { useDutru } from "../../stores/dutru";
import { useGeneral } from "../../stores/general";
import { useMaterials } from "../../stores/materials";
import NsxTreeSelect from "../TreeSelect/NsxTreeSelect.vue";
import MaterialAutoComplete from "../AutoComplete/MaterialAutoComplete.vue";
import { download, formatDate } from "../../utilities/util";
import dutruApi from "../../api/dutruApi";
import { useToast } from "primevue/usetoast";
import ProductTreeSelect from "../TreeSelect/ProductTreeSelect.vue";
import DangbaocheTreeSelect from "../TreeSelect/DangbaocheTreeSelect.vue";
const store_auth = useAuth();
const { user } = storeToRefs(store_auth);
const toast = useToast();
const visible = ref();
const editRow = ref({});
const huyitem = (item) => {
  editRow.value = item;
  visible.value = item;
  // console.log(item);
};
const HuyChitiet = () => {
  if (!editRow.value.note_huy.trim()) {
    alert("Chưa nhập lý do hủy!");
    return false;
  }
  dutruApi
    .huychitiet({
      id: editRow.value.id,
      note_huy: editRow.value.note_huy.trim(),
    })
    .then((response) => {
      // waiting.value = false;
      if (response.success) {
        toast.add({
          severity: "success",
          summary: "Thành công!",
          detail: "Hủy dòng",
          life: 3000,
        });
        location.reload();
      }

      // console.log(response)
    });
};

const rowClass = (data) => {
  return { "bg-gray": data.date_huy ? true : false };
};

const fileChange = (e) => {
  var parents = $(e.target).parents(".custom-file");
  var label = $(".custom-file-label", parents);
  label.text(e.target.files.length + " Files");
};

const xoachitietdinhkem = (index, item) => {
  // console.log(item.dinhkem[key1]);
  confirm.require({
    message: "Bạn có chắc muốn xóa?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      // waiting.value = true;
      dutruApi.xoachitietdinhkem({ id: item[index].id }).then((response) => {
        // waiting.value = false;
        if (response.success) {
          toast.add({
            severity: "success",
            summary: "Thành công!",
            detail: "Xóa đính kèm",
            life: 3000,
          });
          item.splice(index, 1);
        }

        // console.log(response)
      });
    },
  });
};

const label = (row) => {
  return row.mahh ? row.mahh + " - " + row.tenhh : row.tenhh;
};
const select = (event, row) => {
  //   console.log(event);
  var id = event.value.id;
  row.mahh = id;
  row.is_new = false;
  // console.log(row);
  store_general.changeMaterial(row);
};
const store_dutru = useDutru();
const store_general = useGeneral();
const store_materials = useMaterials();
const RefMaterials = storeToRefs(store_materials);
const modelMaterial = RefMaterials.model;
const { headerForm, visibleDialog } = RefMaterials;

const { datatable, model, list_delete } = storeToRefs(store_dutru);
const { materials, products } = storeToRefs(store_general);
const changeMaterial = store_general.changeMaterial;
const changeProducer = store_general.changeProducer;
const confirm = useConfirm();

const loading = ref(false);
const selected = ref();
const columns = computed(() => {
  if (model.value.type_id == 1) {
    return [
      {
        label: "STT(*)",
        data: "stt",
        className: "text-center",
      },
      {
        label: "Hàng hóa(*)",
        data: "tenhh",
        className: "text-center",
      },
      {
        label: "NVL mới(*)",
        data: "is_new",
        className: "text-center",
      },
      {
        label: "Grade(*)",
        data: "grade",
        className: "text-center",
      },
      {
        label: "Tên SP(*)",
        data: "list_sp",
        className: "text-center",
      },
      {
        label: "Dạng bào chế(*)",
        data: "list_dangbaoche",
        className: "text-center",
      },
      {
        label: "ĐVT(*)",
        data: "dvt",
        className: "text-center",
      },
      {
        label: "Số lượng(*)",
        data: "soluong",
        className: "text-center",
      },
      {
        label: "Mã Artwork",
        data: "masothietke",
        className: "text-center",
      },
      {
        label: "Nhà sản xuất",
        data: "nhasx",
        className: "text-center",
      },
      {
        label: "Ngày giao hàng",
        data: "date",
        className: "text-center",
      },
      {
        label: "Mô tả",
        data: "note",
        className: "text-center",
      },
    ];
  } else {
    return [
      {
        label: "STT(*)",
        data: "stt",
        className: "text-center",
      },
      {
        label: "Hàng hóa(*)",
        data: "tenhh",
        className: "text-center",
      },
      {
        label: "ĐVT(*)",
        data: "dvt",
        className: "text-center",
      },
      {
        label: "Số lượng(*)",
        data: "soluong",
        className: "text-center",
      },
      {
        label: "Mô tả",
        data: "note",
        className: "text-center",
      },
      {
        label: "Hình ảnh",
        data: "dinhkem",
        className: "text-center",
      },
    ];
  }
});
const indexActive = ref();
const selectedColumns = computed(() => {
  return columns.value.filter((col) => col.hide != true);
});
const addRow = () => {
  var index = datatable.value.length;
  if (selected.value && selected.value.length == 1) {
    index = selected.value[selected.value.length - 1].stt;
  }
  datatable.value.splice(index, 0, {
    ids: rand(),
    soluong: 1,
    is_new: true,
  });
  calculate_stt();
};
const calculate_stt = () => {
  datatable.value = datatable.value.map((item, index) => {
    item.stt = index + 1;
    return item;
  });
};
const confirmDeleteSelected = () => {
  confirm.require({
    message: "Bạn có chắc muốn xóa những dòng đã chọn?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      datatable.value = datatable.value.filter((item) => {
        return !selected.value.includes(item);
      });

      if (!list_delete.value) {
        list_delete.value = [];
      }
      for (var item of selected.value) {
        if (!item.ids) {
          list_delete.value.push(item);
        }
      }
      selected.value = [];
      // selected
    },
  });
};
const changeProduct = (item, row) => {
  if (item != null && item.length > 0) {
    row.masp = item.join(",");
    var list_dangbaoche = [];
    var product_name = item.map((x) => {
      var product = products.value.find((y) => {
        return y.mahh == x;
      });
      if (product.dangbaoche != null) list_dangbaoche.push(product.dangbaoche);
      return product.tenhh;
    });
    row.tensp = product_name.join(",");
    var uniqueArray = [...new Set(list_dangbaoche)];
    row.list_dangbaoche = uniqueArray;
    changeDangbaoche(row.list_dangbaoche, row);
  } else {
    row.masp = null;
    row.tensp = null;
  }
};
const changeDangbaoche = (item, row) => {
  if (item != null && item.length > 0) {
    row.dangbaoche = item.join(",");
  } else {
    row.dangbaoche = null;
  }
  // console.log(row);
};

onMounted(async () => {
  // if(items)
});
</script>

<style>
.mahh {
  min-width: 300px;
}

.tenhh,
.dinhkem {
  min-width: 300px;
  word-wrap: break-word;
  white-space: break-spaces;
}

.note {
  word-wrap: break-word;
  white-space: pre-line;
}
.list_sp,
.nhasx,
.list_dangbaoche {
  min-width: 300px;
}
.grade {
  min-width: 150px;
}

.date {
  min-width: 200px;
}

.note {
  min-width: 150px;
}
tr.bg-gray {
  background-color: rgb(229 229 229) !important;
}
</style>
