<template>
  <AutoComplete :modelValue="modelValue" @update:modelValue="emit('update:modelValue', $event)" optionLabel="label"
    :disabled="disabled" :suggestions="filterItems" @complete="search" class="w-100" :forceSelection="true"
    :virtualScrollerOptions="{ itemSize: 38 }" inputClass="form-control form-control-sm" :panelStyle="{ width: '50%' }"
    @item-select="emit('item-select', $event)">
    <template #option="slotProps">
      <div class="flex align-options-center">
        {{ slotProps.option.mahh }} - {{ slotProps.option.tenhh }} (<i>Tồn kho</i>: <b>{{ slotProps.option.soluong
        }} {{
            slotProps.option.dvt }}</b>)
      </div>
    </template>
  </AutoComplete>
</template>

<script setup>
import { storeToRefs } from "pinia";
import { computed, onMounted, ref } from "vue";
import AutoComplete from "primevue/autocomplete";
import { useGeneral } from "../../stores/general";
import { FilterMatchMode, FilterService } from "primevue/api";
const props = defineProps({
  modelValue: {
    type: [String],
  },
  type_id: {
    type: [Number, String],
  },
  disabled: {
    type: Boolean,
    default: false,
  },
});
const emit = defineEmits(["update:modelValue", "item-select"]);
const store = useGeneral();
const { NVLGroup } = storeToRefs(store);

const items = computed(() => {
  // console.log("VattuGroup.value", VattuGroup.value);
  return NVLGroup.value.map((item) => {
    item.label = item.mahh + "-" + item.tenhh;
    item.id = item.mahh;
    return item;
  });
});
const filterItems = ref([]);

const search = (event) => {
  let query = event.query;
  let newFiltered = [];

  let filteredItems = FilterService.filter(
    items.value,
    ["label"],
    query,
    FilterMatchMode.CONTAINS
  );
  if (filteredItems && filteredItems.length) {
    newFiltered = filteredItems;
  }

  // console.log(newFiltered);
  filterItems.value = newFiltered;
};
onMounted(() => {
  // store.fetchMaterialGroup();
});
</script>
